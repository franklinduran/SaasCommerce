using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Billing.Domain;

/// <summary>
/// Standard error codes for subscription operations.
/// Used by Result<T> pattern for domain-driven error handling.
/// </summary>
public static class SubscriptionErrors
{
  public static readonly DomainError PlanNotFound = new(
    "subscription.plan_not_found",
    "The specified subscription plan does not exist.");

  public static readonly DomainError PlanNotActive = new(
    "subscription.plan_not_active",
    "The specified subscription plan is not currently available.");

  public static readonly DomainError DuplicatePlanCode = new(
    "subscription.duplicate_plan_code",
    "A subscription plan with the same code already exists.");

  public static readonly DomainError SubscriptionNotFound = new(
    "subscription.not_found",
    "The business does not have an active subscription.");

  public static readonly DomainError SubscriptionExpired = new(
    "subscription.expired",
    "The subscription has expired. Please renew to continue.");

  public static readonly DomainError SubscriptionSuspended = new(
    "subscription.suspended",
    "The subscription is suspended. Contact support to reactivate.");

  public static readonly DomainError SubscriptionCancelled = new(
    "subscription.cancelled",
    "The subscription is cancelled. Start a new subscription to continue.");

  public static readonly DomainError BranchLimitReached = new(
    "subscription.branch_limit_reached",
    "Your subscription plan does not allow creating more branches. Please upgrade your plan.");

  public static readonly DomainError UserLimitReached = new(
    "subscription.user_limit_reached",
    "Your subscription plan does not allow more team members. Please upgrade your plan.");

  public static readonly DomainError ProductLimitReached = new(
    "subscription.product_limit_reached",
    "Your subscription plan does not allow more products. Please upgrade your plan.");

  public static readonly DomainError SalesLimitReached = new(
    "subscription.sales_limit_reached",
    "Your subscription plan has reached the monthly sales limit. Please upgrade your plan.");

  public static readonly DomainError FeatureNotAvailable = new(
    "subscription.feature_not_available",
    "This feature is not available in your current subscription plan. Please upgrade to access it.");

  public static readonly DomainError InvalidSubscriptionTransition = new(
    "subscription.invalid_transition",
    "Cannot perform this operation on a subscription in its current state.");

  public static readonly DomainError UserContextRequired = new(
    "subscription.user_context_required",
    "User context is required for this operation.");

  public static readonly DomainError InvalidPlanData = new(
    "subscription.invalid_plan_data",
    "The provided plan data is invalid.");

  public static readonly DomainError DuplicateSubscription = new(
    "subscription.duplicate",
    "The business already has an active subscription.");

  public static DomainError DowngradeBlocked(string message) => new(
    "subscription.downgrade_blocked",
    message);
}
