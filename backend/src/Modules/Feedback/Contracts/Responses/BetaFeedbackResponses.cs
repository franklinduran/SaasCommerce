namespace SaasCommerce.Modules.Feedback.Contracts.Responses;

public sealed record BetaFeedbackResponse(
  Guid Id,
  Guid BusinessId,
  Guid UserId,
  string Category,
  string Status,
  string Title,
  string Description,
  string? ContextUrl,
  string? ReviewNote,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt,
  DateTimeOffset? ReviewedAt,
  Guid? ReviewedByUserId);

public sealed record BetaFeedbackListResponse(
  IReadOnlyCollection<BetaFeedbackResponse> Items,
  int Page,
  int PageSize,
  int TotalItems,
  int TotalPages,
  bool HasPreviousPage,
  bool HasNextPage);
