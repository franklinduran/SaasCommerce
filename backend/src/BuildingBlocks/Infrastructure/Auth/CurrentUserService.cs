using System.Security.Claims;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Http;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Auth;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
  private const string BusinessIdClaim = "business_id";
  private const string BranchIdClaim = "branch_id";
  private const string UserIdClaim = ClaimTypes.NameIdentifier;
  private const string SubjectClaim = "sub";

  public Guid? UserId => GetGuidClaim(UserIdClaim, SubjectClaim);

  public Guid? BusinessId => GetGuidClaim(BusinessIdClaim);

  public Guid? BranchId => GetGuidClaim(BranchIdClaim);

  public IReadOnlyCollection<string> Roles =>
    httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
      .Select(claim => claim.Value)
      .Where(role => !string.IsNullOrWhiteSpace(role))
      .Distinct(StringComparer.Ordinal)
      .ToArray() ?? [];

  public bool IsAuthenticated =>
    httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

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
