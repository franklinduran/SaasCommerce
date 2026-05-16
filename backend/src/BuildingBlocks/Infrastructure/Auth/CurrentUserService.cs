using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
  private const string BusinessIdClaim = "business_id";
  private const string BranchIdClaim = "branch_id";
  private const string UserIdClaim = ClaimTypes.NameIdentifier;
  private const string SubjectClaim = "sub";
  private string[]? roles;

  public Guid? UserId => GetGuidClaim(UserIdClaim, SubjectClaim);

  public Guid? BusinessId => GetGuidClaim(BusinessIdClaim);

  public Guid? BranchId => GetGuidClaim(BranchIdClaim);

  public IReadOnlyCollection<string> Roles => roles ??= GetRoles();

  public bool IsAuthenticated =>
    httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

  private string[] GetRoles()
    => httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
      .Select(claim => claim.Value)
      .Where(role => !string.IsNullOrWhiteSpace(role))
      .Distinct(StringComparer.Ordinal)
      .ToArray() ?? [];

  private Guid? GetGuidClaim(params string[] claimTypes)
  {
    var user = httpContextAccessor.HttpContext?.User;

    if (user is null)
    {
      return null;
    }

    foreach (var claimType in claimTypes)
    {
      var value = user.FindFirstValue(claimType);

      if (Guid.TryParse(value, out var parsedValue))
      {
        return parsedValue;
      }
    }

    return null;
  }
}
