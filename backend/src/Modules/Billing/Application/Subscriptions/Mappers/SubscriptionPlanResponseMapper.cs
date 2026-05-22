using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;

namespace SaasCommerce.Modules.Billing.Application.Subscriptions.Mappers;

/// <summary>
/// Maps domain SubscriptionPlan entities to response DTOs.
/// </summary>
public static class SubscriptionPlanResponseMapper
{
  public static SubscriptionPlanResponse ToResponse(SubscriptionPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);

    var features = GetEnabledFeatures(plan.Features);

    return new SubscriptionPlanResponse(
      plan.Id,
      plan.Name,
      plan.Code,
      plan.Description,
      plan.MonthlyPrice,
      plan.MaxBranches,
      plan.MaxUsers,
      plan.MaxProducts,
      plan.MaxSalesPerMonth,
      features,
      plan.IsActive,
      plan.CreatedAt,
      plan.UpdatedAt);
  }

  private static List<string> GetEnabledFeatures(SubscriptionFeature features)
  {
    var enabledFeatures = new List<string>();

    if ((features & SubscriptionFeature.Sales) != 0)
      enabledFeatures.Add("Sales");
    if ((features & SubscriptionFeature.Products) != 0)
      enabledFeatures.Add("Products");
    if ((features & SubscriptionFeature.Branches) != 0)
      enabledFeatures.Add("Branches");
    if ((features & SubscriptionFeature.Users) != 0)
      enabledFeatures.Add("Users");
    if ((features & SubscriptionFeature.Purchases) != 0)
      enabledFeatures.Add("Purchases");
    if ((features & SubscriptionFeature.InventoryTransfers) != 0)
      enabledFeatures.Add("InventoryTransfers");
    if ((features & SubscriptionFeature.Invoices) != 0)
      enabledFeatures.Add("Invoices");
    if ((features & SubscriptionFeature.Payments) != 0)
      enabledFeatures.Add("Payments");
    if ((features & SubscriptionFeature.Reports) != 0)
      enabledFeatures.Add("Reports");
    if ((features & SubscriptionFeature.AuditLogs) != 0)
      enabledFeatures.Add("AuditLogs");

    return enabledFeatures;
  }
}

/// <summary>
/// Maps domain BusinessSubscription entities to response DTOs.
/// </summary>
public static class BusinessSubscriptionResponseMapper
{
  public static BusinessSubscriptionResponse ToResponse(
    BusinessSubscription subscription,
    SubscriptionPlan plan)
  {
    ArgumentNullException.ThrowIfNull(subscription);
    ArgumentNullException.ThrowIfNull(plan);

    var planResponse = SubscriptionPlanResponseMapper.ToResponse(plan);

    return new BusinessSubscriptionResponse(
      subscription.Id,
      subscription.BusinessId.Value,
      planResponse,
      subscription.Status.ToString(),
      subscription.StartedAt,
      subscription.TrialEndsAt,
      subscription.CurrentPeriodStart,
      subscription.CurrentPeriodEnd,
      subscription.CancelledAt,
      subscription.SuspendedAt,
      subscription.CancellationReason,
      subscription.CreatedAt,
      subscription.UpdatedAt);
  }
}
