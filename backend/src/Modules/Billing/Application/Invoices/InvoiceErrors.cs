using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public static class InvoiceErrors
{
  public static readonly DomainError UserContextRequired =
    new("invoices.user_context_required", "The authenticated user does not have a valid tenant context.");

  public static readonly DomainError InvalidInvoice =
    new("invoices.invalid_invoice", "The invoice request is invalid.");

  public static readonly DomainError InvalidInvoiceState =
    new("invoices.invalid_state", "The invoice state does not allow this operation.");

  public static readonly DomainError SaleNotFound =
    new("invoices.sale_not_found", "The sale was not found.");

  public static readonly DomainError InvoiceNotFound =
    new("invoices.invoice_not_found", "The invoice was not found.");
}
