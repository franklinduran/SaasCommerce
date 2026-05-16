namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record RegisterBusinessResponse(
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  string AccessToken,
  string RefreshToken,
  DateTimeOffset ExpiresAt,
  AuthUserResponse User);
