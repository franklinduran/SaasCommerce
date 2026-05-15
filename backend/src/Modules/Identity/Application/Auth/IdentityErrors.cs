using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Auth;

public static class IdentityErrors
{
  public static readonly DomainError InvalidCredentials = new(
    "identity.invalid_credentials",
    "Invalid email or password.");

  public static readonly DomainError InvalidRefreshToken = new(
    "identity.invalid_refresh_token",
    "The refresh token is invalid or expired.");

  public static readonly DomainError NotAuthenticated = new(
    "identity.not_authenticated",
    "The current request is not authenticated.");

  public static readonly DomainError UserNotFound = new(
    "identity.user_not_found",
    "The current user was not found.");
}
