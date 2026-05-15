namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record LoginResponse(
  string AccessToken,
  string RefreshToken,
  DateTimeOffset ExpiresAt,
  AuthUserResponse User);
