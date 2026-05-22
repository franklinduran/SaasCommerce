namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Command to reactivate a cancelled business subscription.
/// The subscription can be reactivated with the original plan or a new plan.
/// </summary>
public sealed record ReactivateBusinessSubscriptionCommand(
  Guid? NewPlanId = null,
  Guid? BusinessId = null);
