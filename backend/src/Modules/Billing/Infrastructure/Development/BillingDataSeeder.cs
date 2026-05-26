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
      new SubscriptionPlanDefinition
      {
        Name = "Basic",
        Code = SubscriptionPlanCodes.Basic,
        Description = "Perfect for small businesses just starting out. Includes essential features for managing sales and inventory.",
        MonthlyPrice = 29m,
        MaxBranches = 1,
        MaxUsers = 2,
        MaxProducts = 300,
        MaxSalesPerMonth = 1000,
        Features = SubscriptionFeatures.Sales |
          SubscriptionFeatures.Products |
          SubscriptionFeatures.Branches |
          SubscriptionFeatures.Users |
          SubscriptionFeatures.Purchases |
          SubscriptionFeatures.Invoices |
          SubscriptionFeatures.Payments
      },
      now);

    var proPlan = SubscriptionPlan.Create(
      ProPlanId,
      new SubscriptionPlanDefinition
      {
        Name = "Pro",
        Code = SubscriptionPlanCodes.Pro,
        Description = "Great for growing businesses. Multiple locations, advanced reporting, and inventory transfers.",
        MonthlyPrice = 99m,
        MaxBranches = 3,
        MaxUsers = 10,
        MaxProducts = 2000,
        MaxSalesPerMonth = 10000,
        Features = SubscriptionFeatures.Sales
          | SubscriptionFeatures.Products
          | SubscriptionFeatures.Branches
          | SubscriptionFeatures.Users
          | SubscriptionFeatures.Purchases
          | SubscriptionFeatures.InventoryTransfers
          | SubscriptionFeatures.Invoices
          | SubscriptionFeatures.Payments
          | SubscriptionFeatures.Reports
          | SubscriptionFeatures.AuditLogs
      },
      now);

    var premiumPlan = SubscriptionPlan.Create(
      PremiumPlanId,
      new SubscriptionPlanDefinition
      {
        Name = "Premium",
        Code = SubscriptionPlanCodes.Premium,
        Description = "Enterprise-grade solution with unlimited everything and priority support.",
        MonthlyPrice = 299m,
        MaxBranches = 999,
        MaxUsers = 999,
        MaxProducts = 999999,
        MaxSalesPerMonth = 999999,
        Features = SubscriptionFeatures.All
      },
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
