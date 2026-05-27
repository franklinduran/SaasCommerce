namespace SaasCommerce.Modules.Feedback.Contracts.Requests;

public sealed record CreateBetaFeedbackRequest(
  string Category,
  string Title,
  string Description,
  string? ContextUrl);

public sealed record UpdateBetaFeedbackStatusRequest(
  string Status,
  string? ReviewNote);
