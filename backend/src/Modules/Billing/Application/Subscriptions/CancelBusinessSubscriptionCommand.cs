namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Command to cancel a business's subscription.
/// Cancellation is final and requires explicit reactivation.
/// </summary>
public sealed record CancelBusinessSubscriptionCommand(Guid? BusinessId);
