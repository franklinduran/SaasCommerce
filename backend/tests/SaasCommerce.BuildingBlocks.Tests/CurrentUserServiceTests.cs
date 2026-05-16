using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class CurrentUserServiceTests
{
  [Fact]
  public void CurrentUserServiceShouldReadUserTenantBranchAndRolesFromClaims()
  {
    var userId = Guid.NewGuid();
    var businessId = Guid.NewGuid();
    var branchId = Guid.NewGuid();
    var context = new DefaultHttpContext
    {
      User = new ClaimsPrincipal(new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, userId.ToString("D")),
        new Claim("business_id", businessId.ToString("D")),
        new Claim("branch_id", branchId.ToString("D")),
        new Claim(ClaimTypes.Role, "Admin")
      ], "Test"))
    };
    var service = new CurrentUserService(new HttpContextAccessor { HttpContext = context });

    service.IsAuthenticated.Should().BeTrue();
    service.UserId.Should().Be(userId);
    service.BusinessId.Should().Be(businessId);
    service.BranchId.Should().Be(branchId);
    service.Roles.Should().ContainSingle().Which.Should().Be("Admin");
  }
}
