using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Application.Auth;

internal static class IdentityResponseMapper
{
  public static AuthUserResponse ToAuthUserResponse(User user)
  {
    ArgumentNullException.ThrowIfNull(user);

    return new AuthUserResponse(
      user.Id,
      user.BusinessId.Value,
      user.DefaultBranchId?.Value,
      user.FullName,
      user.Email,
      user.Roles.Select(role => role.Name).Order(StringComparer.Ordinal).ToArray(),
      user.MustChangePassword);
  }
}
