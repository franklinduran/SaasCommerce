using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfSaleReadRepository(AppDbContext dbContext) : ISaleReadRepository
{
  private const string MissingProductName = "Producto no disponible";

  public async Task<SaleResponse?> GetAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
  {
    var header = await BuildHeaderQuery(businessId)
      .SingleOrDefaultAsync(sale => sale.SaleId == saleId, cancellationToken);

    if (header is null)
    {
      return null;
    }

    var items = await ListItemsAsync(businessId, [saleId], cancellationToken);

    return ToResponse(header, items);
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => BuildHeaderQuery(businessId, criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<SaleResponse>> ListAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var headers = await ApplySorting(BuildHeaderQuery(businessId, criteria), criteria)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

    if (headers.Length == 0)
    {
      return [];
    }

    var items = await ListItemsAsync(
      businessId,
      headers.Select(header => header.SaleId).ToArray(),
      cancellationToken);

    return headers
      .Select(header => ToResponse(header, items))
      .ToArray();
  }

  public async Task<IReadOnlyCollection<SaleResponse>> ExportAllAsync(
    BusinessId businessId,
    DateTimeOffset? dateFrom,
    DateTimeOffset? dateTo,
    CancellationToken cancellationToken = default)
  {
    var criteria = new SaleSearchCriteria(
      BranchId: null, Status: null, PaymentMethod: null, Query: null,
      DateFrom: dateFrom, DateTo: dateTo,
      Page: 1, PageSize: int.MaxValue,
      SortBy: SaleSortOption.CreatedAt, SortDirection: SaleSortDirection.Asc);

    var headers = await BuildHeaderQuery(businessId, criteria)
      .OrderBy(s => s.CreatedAt)
      .Take(10_000)
      .ToArrayAsync(cancellationToken);

    if (headers.Length == 0)
    {
      return [];
    }

    var items = await ListItemsAsync(
      businessId,
      headers.Select(h => h.SaleId).ToArray(),
      cancellationToken);

    return headers.Select(h => ToResponse(h, items)).ToArray();
  }

  private IQueryable<SaleHeaderProjection> BuildHeaderQuery(
    BusinessId businessId,
    SaleSearchCriteria? criteria = null)
  {
    var sales = dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(sale => sale.BusinessId == businessId);

    if (criteria is not null)
    {
      sales = ApplySaleFilters(sales, criteria);
    }

    var customers = dbContext.Set<Customer>()
      .AsNoTracking()
      .Where(customer => customer.BusinessId == businessId);
    var branches = dbContext.Set<Branch>()
      .AsNoTracking()
      .Where(branch => branch.BusinessId == businessId);

    var query =
      from sale in sales
      join customer in customers on sale.CustomerId equals customer.Id into customerGroup
      from customer in customerGroup.DefaultIfEmpty()
      join branch in branches on sale.BranchId equals branch.Id into branchGroup
      from branch in branchGroup.DefaultIfEmpty()
      select new
      {
        Sale = sale,
        CustomerName = customer == null ? null : customer.FullName,
        CustomerSearchName = customer == null ? null : customer.SearchName,
        BranchName = branch == null ? null : branch.Name
      };

    if (!string.IsNullOrWhiteSpace(criteria?.Query))
    {
      var term = criteria.Query.Trim();
      var normalizedTerm = term.ToUpperInvariant();
      query = query.Where(row =>
        row.Sale.Id.ToString().Contains(term) ||
        row.CustomerName != null && row.CustomerName.Contains(term) ||
        row.CustomerSearchName != null && row.CustomerSearchName.Contains(normalizedTerm));
    }

    return query.Select(row => new SaleHeaderProjection
    {
      SaleId = row.Sale.Id,
      BusinessId = row.Sale.BusinessId.Value,
      BranchId = row.Sale.BranchId.Value,
      UserId = row.Sale.UserId,
      CustomerId = row.Sale.CustomerId,
      CustomerName = row.CustomerName,
      BranchName = row.BranchName,
      Status = row.Sale.Status.ToString(),
      PaymentMethod = row.Sale.PaymentMethod,
      Total = row.Sale.Total,
      CreatedAt = row.Sale.CreatedAt,
      UpdatedAt = row.Sale.UpdatedAt,
      CompletedAt = row.Sale.CompletedAt,
      FailedAt = row.Sale.FailedAt,
      CancelledAt = row.Sale.CancelledAt,
      FailureReason = row.Sale.FailureReason,
      CancellationReason = row.Sale.CancellationReason
    });
  }

  private static IQueryable<Sale> ApplySaleFilters(
    IQueryable<Sale> query,
    SaleSearchCriteria criteria)
  {
    if (criteria.BranchId.HasValue)
    {
      var branchId = new BranchId(criteria.BranchId.Value);
      query = query.Where(sale => sale.BranchId == branchId);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<SaleStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(sale => sale.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.PaymentMethod))
    {
      query = query.Where(sale => sale.PaymentMethod == criteria.PaymentMethod);
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(sale => sale.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(sale => sale.CreatedAt <= criteria.DateTo.Value);
    }

    return query;
  }

  private static IOrderedQueryable<SaleHeaderProjection> ApplySorting(
    IQueryable<SaleHeaderProjection> query,
    SaleSearchCriteria criteria)
    => (criteria.SortBy, criteria.SortDirection) switch
    {
      (SaleSortOption.Total, SaleSortDirection.Asc) => query.OrderBy(sale => sale.Total),
      (SaleSortOption.Total, _) => query.OrderByDescending(sale => sale.Total),
      (SaleSortOption.CreatedAt, SaleSortDirection.Asc) => query.OrderBy(sale => sale.CreatedAt),
      _ => query.OrderByDescending(sale => sale.CreatedAt)
    };

  private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<SaleItemResponse>>> ListItemsAsync(
    BusinessId businessId,
    IReadOnlyCollection<Guid> saleIds,
    CancellationToken cancellationToken)
  {
    var products = dbContext.Set<Product>()
      .AsNoTracking()
      .Where(product => product.BusinessId == businessId);

    var items = await (
      from item in dbContext.Set<SaleItem>().AsNoTracking()
      where saleIds.Contains(item.SaleId)
      join product in products on item.ProductId equals product.Id into productGroup
      from product in productGroup.DefaultIfEmpty()
      select new SaleItemProjection
      {
        SaleId = item.SaleId,
        SaleItemId = item.Id,
        ProductId = item.ProductId,
        ProductName = product == null ? MissingProductName : product.Name,
        Sku = product == null ? null : product.Sku,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        LineTotal = item.Quantity * item.UnitPrice
      })
      .ToArrayAsync(cancellationToken);

    return items
      .GroupBy(item => item.SaleId)
      .ToDictionary(
        group => group.Key,
        group => (IReadOnlyCollection<SaleItemResponse>)group
          .Select(item => new SaleItemResponse(
            item.SaleItemId,
            item.ProductId,
            item.ProductName,
            item.Sku,
            item.Quantity,
            item.UnitPrice,
            item.LineTotal))
          .ToArray());
  }

  private static SaleResponse ToResponse(
    SaleHeaderProjection header,
    IReadOnlyDictionary<Guid, IReadOnlyCollection<SaleItemResponse>> items)
    => new(
      header.SaleId,
      header.BusinessId,
      header.BranchId,
      header.UserId,
      header.CustomerId,
      header.CustomerName,
      header.BranchName,
      header.Status,
      header.PaymentMethod,
      header.Total,
      items.GetValueOrDefault(header.SaleId, []),
      header.CreatedAt,
      header.UpdatedAt,
      header.CompletedAt,
      header.FailedAt,
      header.CancelledAt,
      header.FailureReason,
      header.CancellationReason);

  private sealed class SaleHeaderProjection
  {
    public Guid SaleId { get; init; }

    public Guid BusinessId { get; init; }

    public Guid BranchId { get; init; }

    public Guid UserId { get; init; }

    public Guid? CustomerId { get; init; }

    public string? CustomerName { get; init; }

    public string? BranchName { get; init; }

    public string Status { get; init; } = string.Empty;

    public string PaymentMethod { get; init; } = string.Empty;

    public decimal Total { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public DateTimeOffset? FailedAt { get; init; }

    public DateTimeOffset? CancelledAt { get; init; }

    public string? FailureReason { get; init; }

    public string? CancellationReason { get; init; }
  }

  private sealed class SaleItemProjection
  {
    public Guid SaleId { get; init; }

    public Guid SaleItemId { get; init; }

    public Guid ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string? Sku { get; init; }

    public decimal Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal LineTotal { get; init; }
  }
}
