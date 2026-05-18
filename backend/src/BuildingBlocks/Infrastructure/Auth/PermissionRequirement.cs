using Microsoft.AspNetCore.Authorization;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Auth;

/// <summary>
/// Authorization requirement that checks for a specific permission code.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
  public string Permission { get; } = permission;
}
