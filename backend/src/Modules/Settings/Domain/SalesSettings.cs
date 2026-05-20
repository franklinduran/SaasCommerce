using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Domain;

/// <summary>
/// Sales and POS operational rules per business tenant.
/// </summary>
public sealed class SalesSettings
{
  private SalesSettings() { }

  public SalesSettings(
    BusinessId businessId,
    bool allowNegativeStock,
    bool allowDiscounts,
    bool requireCustomerForCreditSale,
    string? defaultPaymentMethod,
    bool enableReceiptPrintAfterSale,
    bool enableInvoiceAutoGeneration,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    ArgumentNullException.ThrowIfNull(businessId);

    BusinessId = businessId;
    AllowNegativeStock = allowNegativeStock;
    AllowDiscounts = allowDiscounts;
    RequireCustomerForCreditSale = requireCustomerForCreditSale;
    DefaultPaymentMethod = defaultPaymentMethod?.Trim();
    EnableReceiptPrintAfterSale = enableReceiptPrintAfterSale;
    EnableInvoiceAutoGeneration = enableInvoiceAutoGeneration;
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
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

  public void Update(
    bool allowNegativeStock,
    bool allowDiscounts,
    bool requireCustomerForCreditSale,
    string? defaultPaymentMethod,
    bool enableReceiptPrintAfterSale,
    bool enableInvoiceAutoGeneration,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    AllowNegativeStock = allowNegativeStock;
    AllowDiscounts = allowDiscounts;
    RequireCustomerForCreditSale = requireCustomerForCreditSale;
    DefaultPaymentMethod = defaultPaymentMethod?.Trim();
    EnableReceiptPrintAfterSale = enableReceiptPrintAfterSale;
    EnableInvoiceAutoGeneration = enableInvoiceAutoGeneration;
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
  }

  public static SalesSettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(businessId, false, true, true, null, false, true, userId, now);
}
