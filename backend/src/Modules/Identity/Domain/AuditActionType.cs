namespace SaasCommerce.Modules.Identity.Domain;

public static class AuditActionType
{
  public const string LoginSucceeded = "auth.login_succeeded";
  public const string LoginFailed = "auth.login_failed";
  public const string SaleCreated = "sale.created";
  public const string SaleCompleted = "sale.completed";
  public const string SaleCancelled = "sale.cancelled";
  public const string SaleFailed = "sale.failed";
  public const string PaymentRegistered = "payment.registered";
  public const string InvoiceGenerated = "invoice.generated";
  public const string InventoryAdjusted = "inventory.adjusted";
  public const string PurchaseReceived = "purchase.received";
  public const string PurchaseCancelled = "purchase.cancelled";
  public const string UserCreated = "user.created";
  public const string UserUpdated = "user.updated";
  public const string UserActivated = "user.activated";
  public const string UserDeactivated = "user.deactivated";
  public const string UserRoleChanged = "user.role_changed";
  public const string UserPasswordReset = "user.password_reset";
}
