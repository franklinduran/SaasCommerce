using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Application.Abstractions;

public interface IJwtTokenService
{
  AccessTokenResult CreateAccessToken(User user);
}
