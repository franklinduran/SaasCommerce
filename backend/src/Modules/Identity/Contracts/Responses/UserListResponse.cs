namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record UserListResponse(
  IReadOnlyCollection<UserSummaryResponse> Items,
  int TotalItems);

public sealed record UserSummaryResponse(
  Guid UserId,
  string FullName,
  string Email,
  string? Phone,
  string Role,
  bool IsActive,
  DateTimeOffset CreatedAt);
