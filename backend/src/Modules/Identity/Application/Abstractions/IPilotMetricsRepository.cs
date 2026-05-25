namespace SaasCommerce.Modules.Identity.Application.Abstractions;

public interface IPilotMetricsRepository
{
  Task<PilotMetricsSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public sealed record PilotMetricsSummary(
  int TotalBusinesses,
  int ActiveTrials,
  int ActiveSubscriptions,
  int SuspendedSubscriptions,
  int CancelledSubscriptions,
  int NewBusinessesLast30Days,
  int TrialsExpiringIn7Days,
  IReadOnlyCollection<RecentPilotBusiness> RecentBusinesses);

public sealed record RecentPilotBusiness(
  string BusinessName,
  string SubscriptionStatus,
  string? PlanName,
  DateTimeOffset CreatedAt,
  DateTimeOffset? TrialEndsAt);
