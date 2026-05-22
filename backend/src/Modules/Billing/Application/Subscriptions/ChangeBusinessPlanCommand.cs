namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Command to change a business's subscription plan (upgrade or downgrade).
/// Can be initiated by the business owner or an admin.
/// </summary>
public sealed record ChangeBusinessPlanCommand(
  Guid? BusinessId,
  Guid NewPlanId);
