using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Domain;

/// <summary>
/// Inventory management rules per business tenant.
/// </summary>
public sealed class InventorySettings
{
  private InventorySettings() { }

  public InventorySettings(
    BusinessId businessId,
    bool enableLowStockAlerts,
    decimal defaultLowStockThreshold,
    bool requireReasonForInventoryAdjustment,
    bool allowInventoryTransferBetweenBranches,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    ArgumentNullException.ThrowIfNull(businessId);

    if (defaultLowStockThreshold < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(defaultLowStockThreshold),
        "Default low stock threshold must be >= 0.");
    }

    BusinessId = businessId;
    EnableLowStockAlerts = enableLowStockAlerts;
    DefaultLowStockThreshold = defaultLowStockThreshold;
    RequireReasonForInventoryAdjustment = requireReasonForInventoryAdjustment;
    AllowInventoryTransferBetweenBranches = allowInventoryTransferBetweenBranches;
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
  }

  public BusinessId BusinessId { get; private set; }
  public bool EnableLowStockAlerts { get; private set; } = true;
  public decimal DefaultLowStockThreshold { get; private set; } = 5;
  public bool RequireReasonForInventoryAdjustment { get; private set; }
  public bool AllowInventoryTransferBetweenBranches { get; private set; }
  public Guid UpdatedBy { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }

  public void Update(
    bool enableLowStockAlerts,
    decimal defaultLowStockThreshold,
    bool requireReasonForInventoryAdjustment,
    bool allowInventoryTransferBetweenBranches,
    Guid updatedBy,
    DateTimeOffset updatedAt)
  {
    if (defaultLowStockThreshold < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(defaultLowStockThreshold),
        "Default low stock threshold must be >= 0.");
    }

    EnableLowStockAlerts = enableLowStockAlerts;
    DefaultLowStockThreshold = defaultLowStockThreshold;
    RequireReasonForInventoryAdjustment = requireReasonForInventoryAdjustment;
    AllowInventoryTransferBetweenBranches = allowInventoryTransferBetweenBranches;
    UpdatedBy = updatedBy;
    UpdatedAt = updatedAt;
  }

  public static InventorySettings Default(BusinessId businessId, Guid userId, DateTimeOffset now)
    => new(businessId, true, 5, false, false, userId, now);
}
