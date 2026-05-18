using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Abstractions;

public interface IInvoiceRepository
{
  Task<Invoice?> GetAsync(
    BusinessId businessId,
    Guid invoiceId,
    CancellationToken cancellationToken = default);

  Task<Invoice?> GetBySaleAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task<int> GetNextSequenceAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    InvoiceSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Invoice>> ListAsync(
    BusinessId businessId,
    InvoiceSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
}

public sealed record InvoiceSearchCriteria(
  string? Status,
  string? Query,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);
