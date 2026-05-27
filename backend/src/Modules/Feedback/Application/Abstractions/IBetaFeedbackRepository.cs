using SaasCommerce.Modules.Feedback.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Feedback.Application.Abstractions;

public interface IBetaFeedbackRepository
{
  Task AddAsync(BetaFeedback feedback, CancellationToken cancellationToken = default);

  Task<BetaFeedback?> GetAsync(
    BusinessId businessId,
    Guid feedbackId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<BetaFeedback>> ListAsync(
    BusinessId businessId,
    BetaFeedbackSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    BetaFeedbackSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record BetaFeedbackSearchCriteria(
  BetaFeedbackStatus? Status,
  BetaFeedbackCategory? Category,
  int Page,
  int PageSize);
