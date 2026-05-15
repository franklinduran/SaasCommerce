namespace SaasCommerce.Modules.Identity.Application.Auth;

public sealed record LoginCommand(
  string Email,
  string Password);
