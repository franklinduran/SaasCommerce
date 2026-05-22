using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IBusinessSubscriptionRepository.
/// Persists business subscriptions to the database with tenant isolation.
/// </summary>
public sealed class EfBusinessSubscriptionRepository(AppDbContext context) : IBusinessSubscriptionRepository
{
  public async Task<BusinessSubscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
  {
    return await context.Set<BusinessSubscription>()
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);
  }

  public async Task<BusinessSubscription?> GetCurrentByBusinessIdAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    return await context.Set<BusinessSubscription>()
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.BusinessId == businessId, cancellationToken);
  }

  public async Task<bool> ExistsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
  {
    return await context.Set<BusinessSubscription>()
      .AsNoTracking()
      .AnyAsync(s => s.BusinessId == businessId, cancellationToken);
  }

  public async Task AddAsync(BusinessSubscription subscription, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(subscription);
    await context.Set<BusinessSubscription>().AddAsync(subscription, cancellationToken);
  }

  public Task UpdateAsync(BusinessSubscription subscription, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(subscription);
    context.Set<BusinessSubscription>().Update(subscription);
    return Task.CompletedTask;
  }

  public async Task<IReadOnlyList<BusinessSubscription>> GetByStatusAsync(
    SubscriptionStatus status,
    CancellationToken cancellationToken = default)
  {
    return await context.Set<BusinessSubscription>()
      .AsNoTracking()
      .Where(s => s.Status == status)
      .OrderBy(s => s.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<BusinessSubscription>> GetExpiringAsync(
    DateTimeOffset beforeDate,
    CancellationToken cancellationToken = default)
  {
    return await context.Set<BusinessSubscription>()
      .AsNoTracking()
      .Where(s =>
        (s.Status == SubscriptionStatus.Trial && s.TrialEndsAt != null && s.TrialEndsAt <= beforeDate) ||
        (s.Status == SubscriptionStatus.Active && s.CurrentPeriodEnd != null && s.CurrentPeriodEnd <= beforeDate))
      .OrderBy(s => s.CreatedAt)
      .ToListAsync(cancellationToken);
  }
}
