namespace SaasCommerce.Api.Endpoints;

internal sealed class UpdateBusinessSettingsRequest
{
  public string? CommercialName { get; init; }
  public string? LegalName { get; init; }
  public string? Rnc { get; init; }
  public string? Phone { get; init; }
  public string? Email { get; init; }
  public string? Address { get; init; }
  public string Currency { get; init; } = "DOP";
  public string Timezone { get; init; } = "America/Santo_Domingo";
  public string? LogoUrl { get; init; }
  public string? ReceiptFooterText { get; init; }
}

internal sealed class UpdateSalesSettingsRequest
{
  public bool AllowNegativeStock { get; init; }
  public bool AllowDiscounts { get; init; } = true;
  public bool RequireCustomerForCreditSale { get; init; } = true;
  public string? DefaultPaymentMethod { get; init; }
  public bool EnableReceiptPrintAfterSale { get; init; }
  public bool EnableInvoiceAutoGeneration { get; init; } = true;
}

internal sealed class UpdateInventorySettingsRequest
{
  public bool EnableLowStockAlerts { get; init; } = true;
  public decimal DefaultLowStockThreshold { get; init; } = 5;
  public bool RequireReasonForInventoryAdjustment { get; init; }
  public bool AllowInventoryTransferBetweenBranches { get; init; }
}

internal sealed class UpdateBillingSettingsRequest
{
  public string? ReceiptHeaderText { get; init; }
  public string? ReceiptFooterText { get; init; }
  public bool ShowLogoOnReceipt { get; init; }
  public bool ShowRncOnReceipt { get; init; }
  public bool EnableInvoiceAutoGeneration { get; init; } = true;
  public string InvoicePrefix { get; init; } = "RI";
  public int InvoiceSequenceStart { get; init; } = 1;
}
