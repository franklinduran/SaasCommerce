using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Feedback.Application.Abstractions;
using SaasCommerce.Modules.Feedback.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Feedback.Infrastructure.Persistence;

public sealed class EfBetaFeedbackRepository(AppDbContext db) : IBetaFeedbackRepository
{
  public async Task AddAsync(BetaFeedback feedback, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(feedback);
    await db.Set<BetaFeedback>().AddAsync(feedback, cancellationToken);
  }

  public Task<BetaFeedback?> GetAsync(
    BusinessId businessId,
    Guid feedbackId,
    CancellationToken cancellationToken = default)
    => db.Set<BetaFeedback>()
      .FirstOrDefaultAsync(
        feedback => feedback.BusinessId == businessId && feedback.Id == feedbackId,
        cancellationToken);

  public async Task<IReadOnlyCollection<BetaFeedback>> ListAsync(
    BusinessId businessId,
    BetaFeedbackSearchCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query = ApplyCriteria(businessId, criteria);

    return await query
      .OrderByDescending(feedback => feedback.CreatedAt)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    BetaFeedbackSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyCriteria(businessId, criteria).CountAsync(cancellationToken);

  private IQueryable<BetaFeedback> ApplyCriteria(
    BusinessId businessId,
    BetaFeedbackSearchCriteria criteria)
  {
    var query = db.Set<BetaFeedback>().Where(feedback => feedback.BusinessId == businessId);

    if (criteria.Status.HasValue)
    {
      query = query.Where(feedback => feedback.Status == criteria.Status.Value);
    }

    if (criteria.Category.HasValue)
    {
      query = query.Where(feedback => feedback.Category == criteria.Category.Value);
    }

    return query;
  }
}
