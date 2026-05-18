using FluentAssertions;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Modules.Tests.Identity;

public sealed class SystemRolesTests
{
  [Fact]
  public void AllSetShouldContainExactly7Roles()
  {
    SystemRoles.All.Should().HaveCount(7);
  }

  [Theory]
  [InlineData(SystemRoles.Owner)]
  [InlineData(SystemRoles.Admin)]
  [InlineData(SystemRoles.Supervisor)]
  [InlineData(SystemRoles.Cashier)]
  [InlineData(SystemRoles.InventoryManager)]
  [InlineData(SystemRoles.PurchasingManager)]
  [InlineData(SystemRoles.ReadOnly)]
  public void AllSetShouldContainRole(string roleName)
  {
    SystemRoles.All.Should().Contain(roleName);
  }
}
