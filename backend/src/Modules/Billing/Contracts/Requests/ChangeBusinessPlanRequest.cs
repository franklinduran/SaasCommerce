namespace SaasCommerce.Modules.Billing.Contracts.Requests;

/// <summary>
/// Request to change a business's subscription plan.
/// </summary>
public sealed record ChangeBusinessPlanRequest(
  /// <summary>The ID of the new plan</summary>
  Guid PlanId);

/// <summary>
/// Request to activate a subscription plan (admin only).
/// </summary>
public sealed record ActivateSubscriptionPlanRequest;

/// <summary>
/// Request to deactivate a subscription plan (admin only).
/// </summary>
public sealed record DeactivateSubscriptionPlanRequest;
