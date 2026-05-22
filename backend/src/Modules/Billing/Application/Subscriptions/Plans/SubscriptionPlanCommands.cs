namespace SaasCommerce.Modules.Billing.Application.Subscriptions.Plans;

public sealed record GetSubscriptionPlanByIdQuery(Guid PlanId);

public sealed record CreateSubscriptionPlanCommand(
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

public sealed record UpdateSubscriptionPlanCommand(
  Guid PlanId,
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

public sealed record ActivateSubscriptionPlanCommand(Guid PlanId);

public sealed record DeactivateSubscriptionPlanCommand(Guid PlanId);
