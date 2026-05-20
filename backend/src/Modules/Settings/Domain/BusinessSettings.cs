using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Domain;

/// <summary>
/// Operational and commercial preferences for a business tenant.
/// One row per BusinessId. Missing rows are treated as defaults.
/// </summary>
public sealed class BusinessSettings
{
  private const string DefaultCurrency = "DOP";
  private const string DefaultTimezone = "America/Santo_Domingo";

  private BusinessSettings() { }

  public BusinessSettings(
    BusinessId businessId,
    string? commercialName,
    string? legalName,
    string? rnc,
    string? phone,
    string? email,
    string? address,
    string currency,
    string timezone,
    string? logoUrl,
    string? receiptFooterText,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    ArgumentNullException.ThrowIfNull(businessId);

    BusinessId = businessId;
    CommercialName = commercialName?.Trim();
    LegalName = legalName?.Trim();
    Rnc = rnc?.Trim();
    Phone = phone?.Trim();
    Email = email?.Trim();
    Address = address?.Trim();
    Currency = string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency.Trim().ToUpperInvariant();
    Timezone = string.IsNullOrWhiteSpace(timezone) ? DefaultTimezone : timezone.Trim();
    LogoUrl = logoUrl?.Trim();
    ReceiptFooterText = receiptFooterText?.Trim();
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
  }

  public BusinessId BusinessId { get; private set; }
  public string? CommercialName { get; private set; }
  public string? LegalName { get; private set; }
  public string? Rnc { get; private set; }
  public string? Phone { get; private set; }
  public string? Email { get; private set; }
  public string? Address { get; private set; }
  public string Currency { get; private set; } = DefaultCurrency;
  public string Timezone { get; private set; } = DefaultTimezone;
  public string? LogoUrl { get; private set; }
  public string? ReceiptFooterText { get; private set; }
  public Guid UpdatedBy { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }

  public void Update(
    string? commercialName,
    string? legalName,
    string? rnc,
    string? phone,
    string? email,
    string? address,
    string currency,
    string timezone,
    string? logoUrl,
    string? receiptFooterText,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    CommercialName = commercialName?.Trim();
    LegalName = legalName?.Trim();
    Rnc = rnc?.Trim();
    Phone = phone?.Trim();
    Email = email?.Trim();
    Address = address?.Trim();
    Currency = string.IsNullOrWhiteSpace(currency) ? DefaultCurrency : currency.Trim().ToUpperInvariant();
    Timezone = string.IsNullOrWhiteSpace(timezone) ? DefaultTimezone : timezone.Trim();
    LogoUrl = logoUrl?.Trim();
    ReceiptFooterText = receiptFooterText?.Trim();
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
  }

  public static BusinessSettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(businessId, null, null, null, null, null, null,
        DefaultCurrency, DefaultTimezone, null, null, userId, now);
}
