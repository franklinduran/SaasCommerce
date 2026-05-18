namespace SaasCommerce.Modules.Billing.Contracts.Responses;

public sealed record InvoiceResponse(
  Guid InvoiceId,
  Guid BusinessId,
  Guid BranchId,
  Guid SaleId,
  Guid? CustomerId,
  string InvoiceNumber,
  decimal Subtotal,
  decimal DiscountTotal,
  decimal TaxTotal,
  decimal Total,
  string Status,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt,
  DateTimeOffset? CancelledAt)
{
  public Guid Id => InvoiceId;
}
