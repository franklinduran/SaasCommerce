using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfPilotMetricsRepository(
  AppDbContext dbContext,
  IClock clock) : IPilotMetricsRepository
{
  public async Task<PilotMetricsSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
  {
    var now = clock.UtcNow;
    var thirtyDaysAgo = now.AddDays(-30);
    var sevenDaysFromNow = now.AddDays(7);

    // Aggregate subscription stats
    var subscriptions = await dbContext.Set<BusinessSubscription>()
      .AsNoTracking()
      .Select(sub => new
      {
        sub.BusinessId,
        sub.PlanId,
        sub.Status,
        sub.TrialEndsAt,
        sub.CreatedAt
      })
      .ToArrayAsync(cancellationToken);

    var activeTrials = subscriptions.Count(s => s.Status == SubscriptionStatus.Trial);
    var activeSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
    var suspendedSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Suspended);
    var cancelledSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Cancelled);
    var trialsExpiringIn7Days = subscriptions.Count(s =>
      s.Status == SubscriptionStatus.Trial &&
      s.TrialEndsAt.HasValue &&
      s.TrialEndsAt.Value <= sevenDaysFromNow);

    // Business stats
    var totalBusinesses = await dbContext.Set<Business>()
      .AsNoTracking()
      .CountAsync(cancellationToken);

    var newBusinessesLast30Days = await dbContext.Set<Business>()
      .AsNoTracking()
      .CountAsync(b => b.CreatedAt >= thirtyDaysAgo, cancellationToken);

    // Recent businesses (last 20)
    var recentBusinesses = await (
      from business in dbContext.Set<Business>().AsNoTracking()
      join sub in dbContext.Set<BusinessSubscription>().AsNoTracking()
        on business.Id equals sub.BusinessId into subGroup
      from sub in subGroup.DefaultIfEmpty()
      join plan in dbContext.Set<SubscriptionPlan>().AsNoTracking()
        on (sub == null ? (Guid?)null : (Guid?)sub.PlanId) equals plan.Id into planGroup
      from plan in planGroup.DefaultIfEmpty()
      orderby business.CreatedAt descending
      select new RecentPilotBusiness(
        business.Name,
        sub == null ? "NoSubscription" : sub.Status.ToString(),
        plan == null ? null : plan.Name,
        business.CreatedAt,
        sub == null ? null : sub.TrialEndsAt))
      .Take(20)
      .ToArrayAsync(cancellationToken);

    return new PilotMetricsSummary(
      totalBusinesses,
      activeTrials,
      activeSubscriptions,
      suspendedSubscriptions,
      cancelledSubscriptions,
      newBusinessesLast30Days,
      trialsExpiringIn7Days,
      recentBusinesses);
  }
}
