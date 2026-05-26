namespace SaasCommerce.Modules.Billing.Contracts.Requests;

/// <summary>
/// Request to change a business's subscription plan.
/// </summary>
public sealed record ChangeBusinessPlanRequest(
  /// <summary>The ID of the new plan</summary>
  Guid PlanId);

public sealed record CreateSubscriptionPlanRequest(
  string Name,
  string Code,
  string? Description,
  decimal MonthlyPrice,
  int MaxBranches,
  int MaxUsers,
  int MaxProducts,
  int MaxSalesPerMonth,
  bool AllowInventoryTransfers,
  bool AllowAdvancedReports,
  bool AllowAuditLogs);

public sealed record UpdateSubscriptionPlanRequest(
  string Name,
  string Code,
  string? Description,
  decimal MonthlyPrice,
  int MaxBranches,
  int MaxUsers,
  int MaxProducts,
  int MaxSalesPerMonth,
  bool AllowInventoryTransfers,
  bool AllowAdvancedReports,
  bool AllowAuditLogs);
