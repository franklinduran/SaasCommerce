namespace SaasCommerce.Modules.Identity.Application.Abstractions;

public sealed record AccessTokenResult(
  string Token,
  DateTimeOffset ExpiresAt);
