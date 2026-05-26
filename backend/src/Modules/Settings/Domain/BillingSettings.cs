using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Domain;

/// <summary>
/// Billing, invoice, and receipt configuration per business tenant.
/// </summary>
public sealed class BillingSettings
{
  private const string DefaultInvoicePrefix = "RI";
  private const int DefaultInvoiceSequenceStart = 1;

  private BillingSettings() { }

  public BillingSettings(BillingSettingsDetails details)
  {

    BusinessId = details.BusinessId;
    Apply(details);
  }

  public BusinessId BusinessId { get; private set; }
  public string? ReceiptHeaderText { get; private set; }
  public string? ReceiptFooterText { get; private set; }
  public bool ShowLogoOnReceipt { get; private set; }
  public bool ShowRncOnReceipt { get; private set; }
  public bool EnableInvoiceAutoGeneration { get; private set; } = true;
  public string InvoicePrefix { get; private set; } = DefaultInvoicePrefix;
  public int InvoiceSequenceStart { get; private set; } = DefaultInvoiceSequenceStart;
  public Guid UpdatedBy { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }

  public void Update(BillingSettingsDetails details)
    => Apply(details);

  private void Apply(BillingSettingsDetails details)
  {
    if (details.InvoiceSequenceStart < 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(details),
        "Invoice sequence start must be >= 1.");
    }

    ReceiptHeaderText = details.ReceiptHeaderText?.Trim();
    ReceiptFooterText = details.ReceiptFooterText?.Trim();
    ShowLogoOnReceipt = details.ShowLogoOnReceipt;
    ShowRncOnReceipt = details.ShowRncOnReceipt;
    EnableInvoiceAutoGeneration = details.EnableInvoiceAutoGeneration;
    InvoicePrefix = string.IsNullOrWhiteSpace(details.InvoicePrefix)
      ? DefaultInvoicePrefix
      : details.InvoicePrefix.Trim().ToUpperInvariant();
    InvoiceSequenceStart = details.InvoiceSequenceStart;
    UpdatedBy = details.UpdatedBy;
    UpdatedAt = details.UpdatedAt;
  }

  public static BillingSettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(new BillingSettingsDetails
    {
      BusinessId = businessId,
      ReceiptHeaderText = null,
      ReceiptFooterText = null,
      ShowLogoOnReceipt = false,
      ShowRncOnReceipt = false,
      EnableInvoiceAutoGeneration = true,
      InvoicePrefix = DefaultInvoicePrefix,
      InvoiceSequenceStart = DefaultInvoiceSequenceStart,
      UpdatedBy = userId,
      UpdatedAt = now
    });
}

public sealed class BillingSettingsDetails
{
  public required BusinessId BusinessId { get; init; }
  public string? ReceiptHeaderText { get; init; }
  public string? ReceiptFooterText { get; init; }
  public required bool ShowLogoOnReceipt { get; init; }
  public required bool ShowRncOnReceipt { get; init; }
  public required bool EnableInvoiceAutoGeneration { get; init; }
  public required string InvoicePrefix { get; init; }
  public required int InvoiceSequenceStart { get; init; }
  public required Guid UpdatedBy { get; init; }
  public required DateTimeOffset UpdatedAt { get; init; }
}
