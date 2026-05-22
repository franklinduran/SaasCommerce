using SaasCommerce.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Development;

/// <summary>
/// Seeds initial subscription plan data for development and testing.
/// Creates three plans: Basic, Pro, and Premium with realistic limits.
/// </summary>
public static class BillingDataSeeder
{
  // Plan IDs - these are well-known IDs used in tests and documentation
  public static readonly Guid BasicPlanId = Guid.Parse("11111111-1111-1111-1111-111111111111");
  public static readonly Guid ProPlanId = Guid.Parse("22222222-2222-2222-2222-222222222222");
  public static readonly Guid PremiumPlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");

  /// <summary>
  /// Seed subscription plans into the database if they don't exist.
  /// Safe to call multiple times - only inserts if not present.
  /// </summary>
  public static async Task SeedPlansAsync(DbContext context, DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(context);

    var existingPlans = await context.Set<SubscriptionPlan>()
      .AsNoTracking()
      .CountAsync();

    if (existingPlans > 0)
    {
      // Plans already seeded
      return;
    }

    var basicPlan = SubscriptionPlan.Create(
      BasicPlanId,
      "Basic",
      SubscriptionPlanCodes.Basic,
      "Perfect for small businesses just starting out. Includes essential features for managing sales and inventory.",
      29m,
      maxBranches: 1,
      maxUsers: 2,
      maxProducts: 300,
      maxSalesPerMonth: 1000,
      SubscriptionFeature.Sales |
        SubscriptionFeature.Products |
        SubscriptionFeature.Branches |
        SubscriptionFeature.Users |
        SubscriptionFeature.Purchases |
        SubscriptionFeature.Invoices |
        SubscriptionFeature.Payments,
      now);

    var proPlan = SubscriptionPlan.Create(
      ProPlanId,
      "Pro",
      SubscriptionPlanCodes.Pro,
      "Great for growing businesses. Multiple locations, advanced reporting, and inventory transfers.",
      99m,
      maxBranches: 3,
      maxUsers: 10,
      maxProducts: 2000,
      maxSalesPerMonth: 10000,
      SubscriptionFeature.Sales
        | SubscriptionFeature.Products
        | SubscriptionFeature.Branches
        | SubscriptionFeature.Users
        | SubscriptionFeature.Purchases
        | SubscriptionFeature.InventoryTransfers
        | SubscriptionFeature.Invoices
        | SubscriptionFeature.Payments
        | SubscriptionFeature.Reports
        | SubscriptionFeature.AuditLogs,
      now);

    var premiumPlan = SubscriptionPlan.Create(
      PremiumPlanId,
      "Premium",
      SubscriptionPlanCodes.Premium,
      "Enterprise-grade solution with unlimited everything and priority support.",
      299m,
      maxBranches: 999,
      maxUsers: 999,
      maxProducts: 999999,
      maxSalesPerMonth: 999999,
      SubscriptionFeature.All,
      now);

    await context.Set<SubscriptionPlan>().AddAsync(basicPlan);
    await context.Set<SubscriptionPlan>().AddAsync(proPlan);
    await context.Set<SubscriptionPlan>().AddAsync(premiumPlan);

    await context.SaveChangesAsync();
  }

  /// <summary>
  /// Get the Basic plan ID (for well-known reference in tests).
  /// </summary>
  public static Guid GetBasicPlanId() => BasicPlanId;

  /// <summary>
  /// Get the Pro plan ID (for well-known reference in tests).
  /// </summary>
  public static Guid GetProPlanId() => ProPlanId;

  /// <summary>
  /// Get the Premium plan ID (for well-known reference in tests).
  /// </summary>
  public static Guid GetPremiumPlanId() => PremiumPlanId;
}
