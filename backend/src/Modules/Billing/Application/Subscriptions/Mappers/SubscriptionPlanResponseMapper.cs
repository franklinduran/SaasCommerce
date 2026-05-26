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

  private static List<string> GetEnabledFeatures(SubscriptionFeatures features)
  {
    var enabledFeatures = new List<string>();

    if ((features & SubscriptionFeatures.Sales) != 0)
      enabledFeatures.Add("Sales");
    if ((features & SubscriptionFeatures.Products) != 0)
      enabledFeatures.Add("Products");
    if ((features & SubscriptionFeatures.Branches) != 0)
      enabledFeatures.Add("Branches");
    if ((features & SubscriptionFeatures.Users) != 0)
      enabledFeatures.Add("Users");
    if ((features & SubscriptionFeatures.Purchases) != 0)
      enabledFeatures.Add("Purchases");
    if ((features & SubscriptionFeatures.InventoryTransfers) != 0)
      enabledFeatures.Add("InventoryTransfers");
    if ((features & SubscriptionFeatures.Invoices) != 0)
      enabledFeatures.Add("Invoices");
    if ((features & SubscriptionFeatures.Payments) != 0)
      enabledFeatures.Add("Payments");
    if ((features & SubscriptionFeatures.Reports) != 0)
      enabledFeatures.Add("Reports");
    if ((features & SubscriptionFeatures.AuditLogs) != 0)
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
