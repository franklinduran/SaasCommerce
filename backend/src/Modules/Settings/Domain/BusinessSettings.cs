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

  public BusinessSettings(BusinessSettingsDetails details)
  {
    BusinessId = details.BusinessId;
    Apply(details);
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

  public void Update(BusinessSettingsDetails details)
    => Apply(details);

  private void Apply(BusinessSettingsDetails details)
  {
    CommercialName = details.CommercialName?.Trim();
    LegalName = details.LegalName?.Trim();
    Rnc = details.Rnc?.Trim();
    Phone = details.Phone?.Trim();
    Email = details.Email?.Trim();
    Address = details.Address?.Trim();
    Currency = string.IsNullOrWhiteSpace(details.Currency)
      ? DefaultCurrency
      : details.Currency.Trim().ToUpperInvariant();
    Timezone = string.IsNullOrWhiteSpace(details.Timezone)
      ? DefaultTimezone
      : details.Timezone.Trim();
    LogoUrl = details.LogoUrl?.Trim();
    ReceiptFooterText = details.ReceiptFooterText?.Trim();
    UpdatedBy = details.UpdatedBy;
    UpdatedAt = details.UpdatedAt;
  }

  public static BusinessSettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(new BusinessSettingsDetails
    {
      BusinessId = businessId,
      CommercialName = null,
      LegalName = null,
      Rnc = null,
      Phone = null,
      Email = null,
      Address = null,
      Currency = DefaultCurrency,
      Timezone = DefaultTimezone,
      LogoUrl = null,
      ReceiptFooterText = null,
      UpdatedBy = userId,
      UpdatedAt = now
    });
}

public sealed class BusinessSettingsDetails
{
  public required BusinessId BusinessId { get; init; }
  public string? CommercialName { get; init; }
  public string? LegalName { get; init; }
  public string? Rnc { get; init; }
  public string? Phone { get; init; }
  public string? Email { get; init; }
  public string? Address { get; init; }
  public required string Currency { get; init; }
  public required string Timezone { get; init; }
  public string? LogoUrl { get; init; }
  public string? ReceiptFooterText { get; init; }
  public required Guid UpdatedBy { get; init; }
  public required DateTimeOffset UpdatedAt { get; init; }
}
