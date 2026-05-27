using SaasCommerce.Modules.Feedback.Contracts.Responses;
using SaasCommerce.Modules.Feedback.Domain;

namespace SaasCommerce.Modules.Feedback.Application;

internal static class BetaFeedbackResponseMapper
{
  internal static BetaFeedbackResponse ToResponse(BetaFeedback feedback)
  {
    ArgumentNullException.ThrowIfNull(feedback);

    return new BetaFeedbackResponse(
      feedback.Id,
      feedback.BusinessId.Value,
      feedback.UserId,
      feedback.Category.ToString(),
      feedback.Status.ToString(),
      feedback.Title,
      feedback.Description,
      feedback.ContextUrl,
      feedback.ReviewNote,
      feedback.CreatedAt,
      feedback.UpdatedAt,
      feedback.ReviewedAt,
      feedback.ReviewedByUserId);
  }
}
