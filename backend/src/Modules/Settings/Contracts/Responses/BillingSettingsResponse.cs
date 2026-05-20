namespace SaasCommerce.Modules.Settings.Contracts.Responses;

public sealed record BillingSettingsResponse(
  Guid BusinessId,
  string? ReceiptHeaderText,
  string? ReceiptFooterText,
  bool ShowLogoOnReceipt,
  bool ShowRncOnReceipt,
  bool EnableInvoiceAutoGeneration,
  string InvoicePrefix,
  int InvoiceSequenceStart,
  Guid UpdatedBy,
  DateTimeOffset UpdatedAt);
