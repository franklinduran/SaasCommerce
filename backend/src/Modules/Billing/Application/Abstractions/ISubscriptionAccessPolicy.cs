using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Abstractions;

/// <summary>
/// Service that validates if a business can access a specific feature
/// based on their subscription status and plan.
/// </summary>
public interface ISubscriptionAccessPolicy
{
  /// <summary>
  /// Ensure a business can use a specific feature.
  /// Returns Result.Success if access is allowed, Result.Failure otherwise.
  /// </summary>
  Task<Result> EnsureCanUseFeatureAsync(
    BusinessId businessId,
    SubscriptionFeatures feature,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check the current subscription status for a business.
  /// Used to determine if the subscription is valid before allowing operations.
  /// </summary>
  Task<SubscriptionStatus?> GetCurrentSubscriptionStatusAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business has an active and valid subscription.
  /// </summary>
  Task<bool> IsSubscriptionActiveAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}
