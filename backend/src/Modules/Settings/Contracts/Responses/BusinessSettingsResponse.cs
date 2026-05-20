namespace SaasCommerce.Modules.Settings.Contracts.Responses;

public sealed record BusinessSettingsResponse(
  Guid BusinessId,
  string? CommercialName,
  string? LegalName,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address,
  string Currency,
  string Timezone,
  string? LogoUrl,
  string? ReceiptFooterText,
  Guid UpdatedBy,
  DateTimeOffset UpdatedAt);
