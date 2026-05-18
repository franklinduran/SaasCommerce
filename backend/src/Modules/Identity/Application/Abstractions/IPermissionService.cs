namespace SaasCommerce.Modules.Identity.Application.Abstractions;

/// <summary>
/// Resolves the effective permission set for the currently authenticated user.
/// </summary>
public interface IPermissionService
{
  /// <summary>
  /// Returns all permissions granted to the specified roles.
  /// </summary>
  IReadOnlySet<string> GetPermissionsForRoles(IEnumerable<string> roleNames);

  /// <summary>
  /// Returns true if the specified roles include the given permission.
  /// </summary>
  bool HasPermission(IEnumerable<string> roleNames, string permission);
}
