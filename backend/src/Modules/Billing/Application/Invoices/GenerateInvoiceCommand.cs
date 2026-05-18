namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed record GenerateInvoiceCommand(
  Guid SaleId,
  Guid? CorrelationId = null,
  Guid? BusinessId = null,
  Guid? UserId = null,
  Guid? PaymentId = null,
  bool PublishFailureEvent = false);
