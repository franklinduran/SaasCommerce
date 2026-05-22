using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Abstractions;

/// <summary>
/// Result of a subscription limit check.
/// </summary>
public sealed record SubscriptionLimitCheckResult(
  bool IsAllowed,
  string Code,
  string Message,
  int CurrentUsage,
  int MaxAllowed);

/// <summary>
/// Service that checks if a business has reached subscription limits.
/// </summary>
public interface ISubscriptionLimitChecker
{
  /// <summary>
  /// Check if a business can create a new branch.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business can add a new user/team member.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanCreateUserAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business can create a new product.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanCreateProductAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business can create a new sale this month.
  /// Resets monthly counter on the last day of the month.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanCreateSaleAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business's subscription includes inventory transfer feature.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanUseInventoryTransfersAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business's subscription includes advanced reports.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanUseAdvancedReportsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Check if a business's subscription includes audit logs.
  /// </summary>
  Task<SubscriptionLimitCheckResult> CanUseAuditLogsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}
