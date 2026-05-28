namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Command to change a business's subscription plan (upgrade or downgrade).
/// BusinessId is always resolved from the authenticated user context — never from the caller.
/// </summary>
public sealed record ChangeBusinessPlanCommand(Guid NewPlanId);
