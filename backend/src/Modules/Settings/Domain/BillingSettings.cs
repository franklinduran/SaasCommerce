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

  public BillingSettings( // NOSONAR S107 — settings aggregate requires all fields at construction
    BusinessId businessId,
    string? receiptHeaderText,
    string? receiptFooterText,
    bool showLogoOnReceipt,
    bool showRncOnReceipt,
    bool enableInvoiceAutoGeneration,
    string invoicePrefix,
    int invoiceSequenceStart,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {

    if (invoiceSequenceStart < 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(invoiceSequenceStart),
        "Invoice sequence start must be >= 1.");
    }

    BusinessId = businessId;
    ReceiptHeaderText = receiptHeaderText?.Trim();
    ReceiptFooterText = receiptFooterText?.Trim();
    ShowLogoOnReceipt = showLogoOnReceipt;
    ShowRncOnReceipt = showRncOnReceipt;
    EnableInvoiceAutoGeneration = enableInvoiceAutoGeneration;
    InvoicePrefix = string.IsNullOrWhiteSpace(invoicePrefix)
      ? DefaultInvoicePrefix
      : invoicePrefix.Trim().ToUpperInvariant();
    InvoiceSequenceStart = invoiceSequenceStart;
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
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

  public void Update( // NOSONAR S107 — settings aggregate requires all fields for update
    string? receiptHeaderText,
    string? receiptFooterText,
    bool showLogoOnReceipt,
    bool showRncOnReceipt,
    bool enableInvoiceAutoGeneration,
    string invoicePrefix,
    int invoiceSequenceStart,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    if (invoiceSequenceStart < 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(invoiceSequenceStart),
        "Invoice sequence start must be >= 1.");
    }

    ReceiptHeaderText = receiptHeaderText?.Trim();
    ReceiptFooterText = receiptFooterText?.Trim();
    ShowLogoOnReceipt = showLogoOnReceipt;
    ShowRncOnReceipt = showRncOnReceipt;
    EnableInvoiceAutoGeneration = enableInvoiceAutoGeneration;
    InvoicePrefix = string.IsNullOrWhiteSpace(invoicePrefix)
      ? DefaultInvoicePrefix
      : invoicePrefix.Trim().ToUpperInvariant();
    InvoiceSequenceStart = invoiceSequenceStart;
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
  }

  public static BillingSettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(businessId, null, null, false, false, true,
        DefaultInvoicePrefix, DefaultInvoiceSequenceStart, userId, now);
}
