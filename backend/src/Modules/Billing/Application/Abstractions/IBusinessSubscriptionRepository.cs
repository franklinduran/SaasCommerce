using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Abstractions;

/// <summary>
/// Repository interface for business subscription persistence.
/// Each business has exactly one subscription at any given time.
/// </summary>
public interface IBusinessSubscriptionRepository
{
  /// <summary>
  /// Get a specific subscription by ID.
  /// </summary>
  Task<BusinessSubscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get the current subscription for a business.
  /// Returns null if the business has no subscription.
  /// </summary>
  Task<BusinessSubscription?> GetCurrentByBusinessIdAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business already has a subscription.
  /// </summary>
  Task<bool> ExistsAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Add a new subscription for a business.
  /// </summary>
  Task AddAsync(BusinessSubscription subscription, CancellationToken cancellationToken = default);

  /// <summary>
  /// Update an existing subscription.
  /// </summary>
  Task UpdateAsync(BusinessSubscription subscription, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get all subscriptions with a specific status.
  /// Useful for batch operations (e.g., expiring trials, suspending past due, etc.)
  /// </summary>
  Task<IReadOnlyList<BusinessSubscription>> GetByStatusAsync(
    SubscriptionStatus status,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Get all subscriptions that should expire on or before the given date.
  /// </summary>
  Task<IReadOnlyList<BusinessSubscription>> GetExpiringAsync(
    DateTimeOffset beforeDate,
    CancellationToken cancellationToken = default);
}
