namespace SaasCommerce.Modules.Billing.Application.Abstractions;

public interface IInvoiceSaleReader
{
  Task<InvoiceSaleSnapshot?> GetAsync(
    Guid businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);
}

public sealed record InvoiceSaleSnapshot(
  Guid SaleId,
  Guid BusinessId,
  Guid BranchId,
  Guid? CustomerId,
  string Status,
  decimal Total);
