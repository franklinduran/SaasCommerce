using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfSaleReturnReadRepository(AppDbContext dbContext) : ISaleReturnReadRepository
{
  private const string MissingProductName = "Producto no disponible";

  public async Task<SaleReturnResponse?> GetAsync(
    BusinessId businessId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default)
  {
    var saleReturn = await BaseReturns(businessId)
      .SingleOrDefaultAsync(item => item.Id == saleReturnId, cancellationToken);

    return saleReturn is null
      ? null
      : await BuildResponseAsync(businessId, saleReturn, cancellationToken);
  }

  public async Task<IReadOnlyCollection<SaleReturnResponse>> ListBySaleAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
  {
    var saleReturns = await BaseReturns(businessId)
      .Where(saleReturn => saleReturn.SaleId == saleId)
      .OrderByDescending(saleReturn => saleReturn.RequestedAt)
      .ToArrayAsync(cancellationToken);

    var responses = new List<SaleReturnResponse>(saleReturns.Length);
    foreach (var saleReturn in saleReturns)
    {
      responses.Add(await BuildResponseAsync(businessId, saleReturn, cancellationToken));
    }

    return responses;
  }

  private IQueryable<SaleReturn> BaseReturns(BusinessId businessId)
    => dbContext.Set<SaleReturn>()
      .AsNoTracking()
      .Include(saleReturn => saleReturn.Items)
      .Where(saleReturn => saleReturn.BusinessId == businessId);

  private async Task<SaleReturnResponse> BuildResponseAsync(
    BusinessId businessId,
    SaleReturn saleReturn,
    CancellationToken cancellationToken)
  {
    var productIds = saleReturn.Items.Select(item => item.ProductId).ToArray();
    var products = await dbContext.Set<Product>()
      .AsNoTracking()
      .Where(product => product.BusinessId == businessId && productIds.Contains(product.Id))
      .ToDictionaryAsync(product => product.Id, cancellationToken);

    var creditNote = await dbContext.Set<CreditNote>()
      .AsNoTracking()
      .Include(note => note.Items)
      .SingleOrDefaultAsync(
        note => note.BusinessId == businessId && note.SaleReturnId == saleReturn.Id,
        cancellationToken);

    return new SaleReturnResponse(
      saleReturn.Id,
      saleReturn.SaleId,
      saleReturn.BusinessId.Value,
      saleReturn.BranchId.Value,
      saleReturn.UserId,
      saleReturn.Status.ToString(),
      saleReturn.Reason,
      saleReturn.Total,
      saleReturn.Items.Select(item => ToReturnItemResponse(item, products)).ToArray(),
      creditNote is null ? null : ToCreditNoteResponse(creditNote, products),
      saleReturn.RequestedAt,
      saleReturn.ApprovedAt,
      saleReturn.FailedAt,
      saleReturn.FailureReason);
  }

  private static SaleReturnItemResponse ToReturnItemResponse(
    SaleReturnItem item,
    Dictionary<Guid, Product> products)
  {
    products.TryGetValue(item.ProductId, out var product);
    return new SaleReturnItemResponse(
      item.Id,
      item.SaleItemId,
      item.ProductId,
      product?.Name ?? MissingProductName,
      product?.Sku,
      item.Quantity,
      item.UnitPrice,
      item.LineTotal);
  }

  private static CreditNoteResponse ToCreditNoteResponse(
    CreditNote creditNote,
    Dictionary<Guid, Product> products)
    => new(
      creditNote.Id,
      creditNote.SaleId,
      creditNote.SaleReturnId,
      creditNote.CustomerId,
      creditNote.Code,
      creditNote.Total,
      creditNote.Items.Select(item =>
      {
        products.TryGetValue(item.ProductId, out var product);
        return new CreditNoteItemResponse(
          item.Id,
          item.ProductId,
          product?.Name ?? MissingProductName,
          product?.Sku,
          item.Quantity,
          item.UnitPrice,
          item.LineTotal);
      }).ToArray(),
      creditNote.CreatedAt);
}
