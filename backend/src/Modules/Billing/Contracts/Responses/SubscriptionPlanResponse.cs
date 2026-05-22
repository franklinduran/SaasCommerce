namespace SaasCommerce.Modules.Billing.Contracts.Responses;

/// <summary>
/// DTO for subscription plan response to API consumers.
/// </summary>
public sealed record SubscriptionPlanResponse(
  Guid Id,
  string Name,
  string Code,
  string Description,
  decimal MonthlyPrice,
  int MaxBranches,
  int MaxUsers,
  int MaxProducts,
  int MaxSalesPerMonth,
  IReadOnlyList<string> Features,
  bool IsActive,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);

/// <summary>
/// DTO for business subscription response to API consumers.
/// </summary>
public sealed record BusinessSubscriptionResponse(
  Guid Id,
  Guid BusinessId,
  SubscriptionPlanResponse Plan,
  string Status,
  DateTimeOffset StartedAt,
  DateTimeOffset? TrialEndsAt,
  DateTimeOffset? CurrentPeriodStart,
  DateTimeOffset? CurrentPeriodEnd,
  DateTimeOffset? CancelledAt,
  DateTimeOffset? SuspendedAt,
  string? CancellationReason,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);

/// <summary>
/// DTO for subscription usage/limits response.
/// Shows current usage and maximum limits for countable resources.
/// </summary>
public sealed record SubscriptionUsageResponse(
  Guid SubscriptionId,
  string PlanName,
  string Status,
  BranchUsage Branches,
  UserUsage Users,
  ProductUsage Products,
  SalesUsage Sales,
  IReadOnlyList<FeatureStatus> Features,
  DateTimeOffset? TrialEndsAt,
  DateTimeOffset? PeriodEndsAt);

public sealed record BranchUsage(int Current, int Maximum, bool IsAtLimit);
public sealed record UserUsage(int Current, int Maximum, bool IsAtLimit);
public sealed record ProductUsage(int Current, int Maximum, bool IsAtLimit);
public sealed record SalesUsage(int Current, int Maximum, bool IsAtLimit);
public sealed record FeatureStatus(string Name, bool IsEnabled);
