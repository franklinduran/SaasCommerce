using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ISubscriptionPlanRepository.
/// Persists subscription plans to the database.
/// </summary>
public sealed class EfSubscriptionPlanRepository(AppDbContext context) : ISubscriptionPlanRepository
{
  public async Task<SubscriptionPlan?> GetByIdAsync(Guid planId, CancellationToken cancellationToken = default)
  {
    return await context.Set<SubscriptionPlan>()
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
  }

  public async Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
  {
    var normalizedCode = code.Trim().ToUpperInvariant();

    return await context.Set<SubscriptionPlan>()
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Code == normalizedCode, cancellationToken);
  }

  public async Task<IReadOnlyList<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
  {
    return await context.Set<SubscriptionPlan>()
      .AsNoTracking()
      .Where(p => p.IsActive)
      .OrderBy(p => p.MonthlyPrice)
      .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    return await context.Set<SubscriptionPlan>()
      .AsNoTracking()
      .OrderBy(p => p.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task AddAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(plan);
    await context.Set<SubscriptionPlan>().AddAsync(plan, cancellationToken);
  }

  public Task UpdateAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(plan);
    context.Set<SubscriptionPlan>().Update(plan);
    return Task.CompletedTask;
  }
}
