namespace SaasCommerce.Modules.Settings.Contracts.Responses;

public sealed record SalesSettingsResponse(
  Guid BusinessId,
  bool AllowNegativeStock,
  bool AllowDiscounts,
  bool RequireCustomerForCreditSale,
  string? DefaultPaymentMethod,
  bool EnableReceiptPrintAfterSale,
  bool EnableInvoiceAutoGeneration,
  Guid UpdatedBy,
  DateTimeOffset UpdatedAt);
