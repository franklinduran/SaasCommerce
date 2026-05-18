using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

internal static class InvoiceResponseMapper
{
  public static InvoiceResponse ToResponse(Invoice invoice)
  {
    ArgumentNullException.ThrowIfNull(invoice);

    return new InvoiceResponse(
      invoice.Id,
      invoice.BusinessId.Value,
      invoice.BranchId.Value,
      invoice.SaleId,
      invoice.CustomerId,
      invoice.InvoiceNumber,
      invoice.Subtotal,
      invoice.DiscountTotal,
      invoice.TaxTotal,
      invoice.Total,
      invoice.Status.ToString(),
      invoice.CreatedAt,
      invoice.UpdatedAt,
      invoice.CancelledAt);
  }
}
