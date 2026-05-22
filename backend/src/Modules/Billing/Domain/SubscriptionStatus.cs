namespace SaasCommerce.Modules.Billing.Domain;

/// <summary>
/// Represents the lifecycle states of a business subscription.
/// Trial → Active → Expired → Suspended (optional) → Cancelled (final state)
/// PastDue is a transient state before Suspended.
/// </summary>
public enum SubscriptionStatus
{
  /// <summary>Free trial period (default for new businesses, 14 days)</summary>
  Trial,

  /// <summary>Active paid subscription or trial in progress</summary>
  Active,

  /// <summary>Payment is overdue but not yet suspended (grace period)</summary>
  PastDue,

  /// <summary>Subscription temporarily suspended due to non-payment or policy violation</summary>
  Suspended,

  /// <summary>Trial or subscription period has expired without renewal</summary>
  Expired,

  /// <summary>Cancelled subscription (final state, cannot be reverted without explicit reactivation)</summary>
  Cancelled
}
