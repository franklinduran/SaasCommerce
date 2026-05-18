using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Infrastructure.Auth;

/// <summary>
/// Resolves permissions from the static <see cref="RolePermissionMatrix"/>.
/// No database queries required — the matrix is code-defined for the MVP.
/// </summary>
public sealed class PermissionService : IPermissionService
{
  public IReadOnlySet<string> GetPermissionsForRoles(IEnumerable<string> roleNames)
    => RolePermissionMatrix.GetPermissions(roleNames);

  public bool HasPermission(IEnumerable<string> roleNames, string permission)
    => GetPermissionsForRoles(roleNames).Contains(permission);
}
