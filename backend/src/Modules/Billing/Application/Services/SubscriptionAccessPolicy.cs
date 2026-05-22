using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Services;

/// <summary>
/// Implements subscription access control based on plan features and subscription status.
/// Validates that a business can use specific features.
/// </summary>
public sealed class SubscriptionAccessPolicy(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  IClock clock) : ISubscriptionAccessPolicy
{
  public async Task<Result> EnsureCanUseFeatureAsync(
    BusinessId businessId,
    SubscriptionFeature feature,
    CancellationToken cancellationToken = default)
  {
    var subscription = await subscriptionRepository.GetCurrentByBusinessIdAsync(businessId, cancellationToken);

    if (subscription is null)
    {
      return Result.Failure(SubscriptionErrors.SubscriptionNotFound);
    }

    // Check subscription status
    var statusCheck = ValidateSubscriptionStatus(subscription, clock.UtcNow);
    if (statusCheck.IsFailure)
    {
      return statusCheck;
    }

    // Get the plan and check if it has the feature
    var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure(SubscriptionErrors.PlanNotFound);
    }

    if (!plan.HasFeature(feature))
    {
      return Result.Failure(SubscriptionErrors.FeatureNotAvailable);
    }

    return Result.Success();
  }

  public async Task<SubscriptionStatus?> GetCurrentSubscriptionStatusAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var subscription = await subscriptionRepository.GetCurrentByBusinessIdAsync(businessId, cancellationToken);

    if (subscription is null)
    {
      return null;
    }

    // Check if subscription has expired
    var now = clock.UtcNow;
    if (subscription.IsExpiredOrShouldExpire(now))
    {
      subscription.MarkExpired(now);
      await subscriptionRepository.UpdateAsync(subscription, cancellationToken);
      return SubscriptionStatus.Expired;
    }

    return subscription.Status;
  }

  public async Task<bool> IsSubscriptionActiveAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var status = await GetCurrentSubscriptionStatusAsync(businessId, cancellationToken);
    return status == SubscriptionStatus.Active;
  }

  private static Result ValidateSubscriptionStatus(
    BusinessSubscription subscription,
    DateTimeOffset now)
  {
    if (subscription.IsExpiredOrShouldExpire(now))
    {
      return Result.Failure(SubscriptionErrors.SubscriptionExpired);
    }

    return subscription.Status switch
    {
      SubscriptionStatus.Active => Result.Success(),
      SubscriptionStatus.Trial => Result.Success(),
      SubscriptionStatus.Expired => Result.Failure(SubscriptionErrors.SubscriptionExpired),
      SubscriptionStatus.Suspended => Result.Failure(SubscriptionErrors.SubscriptionSuspended),
      SubscriptionStatus.Cancelled => Result.Failure(SubscriptionErrors.SubscriptionCancelled),
      SubscriptionStatus.PastDue => Result.Failure(SubscriptionErrors.SubscriptionSuspended),
      _ => Result.Failure(new DomainError("subscription.unknown_status", "Unknown subscription status."))
    };
  }
}
