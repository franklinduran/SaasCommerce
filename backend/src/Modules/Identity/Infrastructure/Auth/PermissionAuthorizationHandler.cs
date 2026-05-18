using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Infrastructure.Auth;

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> by resolving the user's role claims
/// against the static <see cref="RolePermissionMatrix"/>.
/// </summary>
public sealed class PermissionAuthorizationHandler
  : AuthorizationHandler<PermissionRequirement>
{
  protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement)
  {
    var roleNames = context.User
      .FindAll(ClaimTypes.Role)
      .Select(c => c.Value)
      .Where(v => !string.IsNullOrWhiteSpace(v));

    if (RolePermissionMatrix.GetPermissions(roleNames).Contains(requirement.Permission))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}
