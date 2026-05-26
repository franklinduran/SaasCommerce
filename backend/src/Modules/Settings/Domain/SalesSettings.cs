using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Domain;

/// <summary>
/// Sales and POS operational rules per business tenant.
/// </summary>
public sealed class SalesSettings
{
  private SalesSettings() { }

  public SalesSettings(SalesSettingsDetails details)
  {
    BusinessId = details.BusinessId;
    Apply(details);
  }

  public BusinessId BusinessId { get; private set; }
  public bool AllowNegativeStock { get; private set; }
  public bool AllowDiscounts { get; private set; } = true;
  public bool RequireCustomerForCreditSale { get; private set; } = true;
  public string? DefaultPaymentMethod { get; private set; }
  public bool EnableReceiptPrintAfterSale { get; private set; }
  public bool EnableInvoiceAutoGeneration { get; private set; } = true;
  public Guid UpdatedBy { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }

  public void Update(SalesSettingsDetails details)
    => Apply(details);

  private void Apply(SalesSettingsDetails details)
  {
    AllowNegativeStock = details.AllowNegativeStock;
    AllowDiscounts = details.AllowDiscounts;
    RequireCustomerForCreditSale = details.RequireCustomerForCreditSale;
    DefaultPaymentMethod = details.DefaultPaymentMethod?.Trim();
    EnableReceiptPrintAfterSale = details.EnableReceiptPrintAfterSale;
    EnableInvoiceAutoGeneration = details.EnableInvoiceAutoGeneration;
    UpdatedBy = details.UpdatedBy;
    UpdatedAt = details.UpdatedAt;
  }

  public static SalesSettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(new SalesSettingsDetails
    {
      BusinessId = businessId,
      AllowNegativeStock = false,
      AllowDiscounts = true,
      RequireCustomerForCreditSale = true,
      DefaultPaymentMethod = null,
      EnableReceiptPrintAfterSale = false,
      EnableInvoiceAutoGeneration = true,
      UpdatedBy = userId,
      UpdatedAt = now
    });
}

public sealed class SalesSettingsDetails
{
  public required BusinessId BusinessId { get; init; }
  public required bool AllowNegativeStock { get; init; }
  public required bool AllowDiscounts { get; init; }
  public required bool RequireCustomerForCreditSale { get; init; }
  public string? DefaultPaymentMethod { get; init; }
  public required bool EnableReceiptPrintAfterSale { get; init; }
  public required bool EnableInvoiceAutoGeneration { get; init; }
  public required Guid UpdatedBy { get; init; }
  public required DateTimeOffset UpdatedAt { get; init; }
}
