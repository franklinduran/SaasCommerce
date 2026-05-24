namespace SaasCommerce.Modules.Identity.Application.Auth;

/// <summary>
/// Revokes the supplied refresh token so it cannot be used to obtain new access tokens.
/// The current access token remains valid until its natural expiry (short-lived by design).
/// </summary>
public sealed record LogoutCommand(string RefreshToken);
