namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record UserDetailResponse(
  Guid Id,
  string FullName,
  string Email,
  string? Phone,
  string Role,
  Guid? DefaultBranchId,
  bool IsActive,
  bool MustChangePassword,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);
