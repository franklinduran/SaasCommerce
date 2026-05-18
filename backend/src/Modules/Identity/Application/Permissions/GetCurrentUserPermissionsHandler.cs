using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Permissions;

public sealed record GetCurrentUserPermissionsQuery;

public sealed class GetCurrentUserPermissionsHandler(
  ICurrentUserService currentUser,
  IPermissionService permissionService)
{
  public Result<CurrentUserPermissionsResponse> Handle(GetCurrentUserPermissionsQuery query)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated ||
        currentUser.UserId is not { } userId ||
        currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<CurrentUserPermissionsResponse>(
        IdentityPermissionsErrors.UserContextRequired);
    }

    var roles = currentUser.Roles;
    var primaryRole = roles.FirstOrDefault() ?? string.Empty;
    var permissions = permissionService.GetPermissionsForRoles(roles);

    return Result.Success(new CurrentUserPermissionsResponse(
      userId,
      businessId,
      primaryRole,
      permissions.OrderBy(p => p, StringComparer.Ordinal).ToArray()));
  }
}
