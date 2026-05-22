using SaasCommerce.Modules.Billing.Domain;

namespace SaasCommerce.Modules.Billing.Application.Abstractions;

/// <summary>
/// Repository interface for subscription plan persistence.
/// Plans are global (not tenant-specific).
/// </summary>
public interface ISubscriptionPlanRepository
{
  /// <summary>
  /// Get a specific plan by ID.
  /// </summary>
  Task<SubscriptionPlan?> GetByIdAsync(Guid planId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a specific plan by its stable code.
  /// </summary>
  Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get all active plans available for new subscriptions.
  /// </summary>
  Task<IReadOnlyList<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Get all plans (active and inactive).
  /// </summary>
  Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Add a new plan to the database.
  /// </summary>
  Task AddAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);

  /// <summary>
  /// Update an existing plan.
  /// </summary>
  Task UpdateAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);
}
