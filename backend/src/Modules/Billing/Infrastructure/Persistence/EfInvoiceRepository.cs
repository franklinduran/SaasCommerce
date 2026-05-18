using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence;

public sealed class EfInvoiceRepository(AppDbContext dbContext) : IInvoiceRepository
{
  public Task<Invoice?> GetAsync(
    BusinessId businessId,
    Guid invoiceId,
    CancellationToken cancellationToken = default)
    => Invoices(businessId)
      .SingleOrDefaultAsync(invoice => invoice.Id == invoiceId, cancellationToken);

  public Task<Invoice?> GetBySaleAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
    => Invoices(businessId)
      .SingleOrDefaultAsync(invoice => invoice.SaleId == saleId, cancellationToken);

  public async Task<int> GetNextSequenceAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var current = await Invoices(businessId)
      .Select(invoice => (int?)invoice.Sequence)
      .MaxAsync(cancellationToken);

    return (current ?? 0) + 1;
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    InvoiceSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Invoices(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Invoice>> ListAsync(
    BusinessId businessId,
    InvoiceSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplyFilters(Invoices(businessId).AsNoTracking(), criteria)
      .OrderByDescending(invoice => invoice.CreatedAt)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(invoice);

    return dbContext.Set<Invoice>().AddAsync(invoice, cancellationToken).AsTask();
  }

  private IQueryable<Invoice> Invoices(BusinessId businessId)
    => dbContext.Set<Invoice>()
      .Where(invoice => invoice.BusinessId == businessId);

  private static IQueryable<Invoice> ApplyFilters(
    IQueryable<Invoice> query,
    InvoiceSearchCriteria criteria)
  {
    if (!string.IsNullOrWhiteSpace(criteria.Status) &&
        Enum.TryParse<InvoiceStatus>(criteria.Status, true, out var status))
    {
      query = query.Where(invoice => invoice.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Query))
    {
      var normalizedQuery = criteria.Query.Trim();
      query = query.Where(invoice =>
        invoice.InvoiceNumber.Contains(normalizedQuery) ||
        invoice.SaleId.ToString().Contains(normalizedQuery));
    }

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(invoice => invoice.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(invoice => invoice.CreatedAt <= criteria.DateTo.Value);
    }

    return query;
  }
}
