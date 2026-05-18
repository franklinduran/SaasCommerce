namespace SaasCommerce.Modules.Identity.Contracts;

/// <summary>
/// Well-known role names used throughout the system.
/// Roles are stored per-business in the database; this class defines the canonical names.
/// </summary>
public static class SystemRoles
{
  public const string Owner = "Owner";
  public const string Admin = "Admin";
  public const string Supervisor = "Supervisor";
  public const string Cashier = "Cashier";
  public const string InventoryManager = "InventoryManager";
  public const string PurchasingManager = "PurchasingManager";
  public const string ReadOnly = "ReadOnly";

  public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
  {
    Owner, Admin, Supervisor, Cashier, InventoryManager, PurchasingManager, ReadOnly
  };
}
