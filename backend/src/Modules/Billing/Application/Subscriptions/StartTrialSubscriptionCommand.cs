namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Command to start a trial subscription for a new business.
/// Only called once when a business first registers.
/// </summary>
public sealed record StartTrialSubscriptionCommand(Guid? BusinessId);
