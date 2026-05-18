using FluentAssertions;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Tests.Identity;

public sealed class RolePermissionMatrixTests
{
  [Fact]
  public void OwnerShouldHaveAllPermissions()
  {
    var permissions = RolePermissionMatrix.GetPermissions(SystemRoles.Owner);

    permissions.Should().Contain(SystemPermissions.DashboardView);
    permissions.Should().Contain(SystemPermissions.SalesCreate);
    permissions.Should().Contain(SystemPermissions.UsersUpdateRole);
    permissions.Should().Contain(SystemPermissions.AuditView);
    permissions.Should().Contain(SystemPermissions.ReportsExport);
  }

  [Fact]
  public void AdminShouldHaveAllPermissions()
  {
    var permissions = RolePermissionMatrix.GetPermissions(SystemRoles.Admin);

    permissions.Should().Contain(SystemPermissions.DashboardView);
    permissions.Should().Contain(SystemPermissions.SalesCreate);
    permissions.Should().Contain(SystemPermissions.UsersUpdateRole);
    permissions.Should().Contain(SystemPermissions.AuditView);
  }

  [Fact]
  public void CashierShouldHavePosPermissionsOnly()
  {
    var permissions = RolePermissionMatrix.GetPermissions(SystemRoles.Cashier);

    permissions.Should().Contain(SystemPermissions.SalesCreate);
    permissions.Should().Contain(SystemPermissions.SalesView);
    permissions.Should().Contain(SystemPermissions.ProductsView);
    permissions.Should().NotContain(SystemPermissions.UsersUpdateRole);
    permissions.Should().NotContain(SystemPermissions.AuditView);
    permissions.Should().NotContain(SystemPermissions.ReportsExport);
    permissions.Should().NotContain(SystemPermissions.PurchasesCreate);
  }

  [Fact]
  public void ReadOnlyShouldHaveViewPermissionsOnly()
  {
    var permissions = RolePermissionMatrix.GetPermissions(SystemRoles.ReadOnly);

    permissions.Should().Contain(SystemPermissions.DashboardView);
    permissions.Should().Contain(SystemPermissions.SalesView);
    permissions.Should().Contain(SystemPermissions.ProductsView);
    permissions.Should().NotContain(SystemPermissions.SalesCreate);
    permissions.Should().NotContain(SystemPermissions.SalesCancel);
    permissions.Should().NotContain(SystemPermissions.ProductsCreate);
    permissions.Should().NotContain(SystemPermissions.InventoryAdjust);
  }

  [Fact]
  public void InventoryManagerShouldHaveWarehousePermissions()
  {
    var permissions = RolePermissionMatrix.GetPermissions(SystemRoles.InventoryManager);

    permissions.Should().Contain(SystemPermissions.InventoryView);
    permissions.Should().Contain(SystemPermissions.InventoryAdjust);
    permissions.Should().Contain(SystemPermissions.ProductsView);
    permissions.Should().NotContain(SystemPermissions.SalesCreate);
    permissions.Should().NotContain(SystemPermissions.UsersUpdateRole);
  }

  [Fact]
  public void PurchasingManagerShouldHavePurchasePermissions()
  {
    var permissions = RolePermissionMatrix.GetPermissions(SystemRoles.PurchasingManager);

    permissions.Should().Contain(SystemPermissions.PurchasesCreate);
    permissions.Should().Contain(SystemPermissions.PurchasesReceive);
    permissions.Should().Contain(SystemPermissions.PurchasesCancel);
    permissions.Should().NotContain(SystemPermissions.SalesCreate);
    permissions.Should().NotContain(SystemPermissions.UsersUpdateRole);
  }

  [Fact]
  public void UnknownRoleShouldReturnEmptyPermissions()
  {
    var permissions = RolePermissionMatrix.GetPermissions("NonExistentRole");

    permissions.Should().BeEmpty();
  }

  [Fact]
  public void MultipleRolesShouldReturnUnionOfPermissions()
  {
    var permissions = RolePermissionMatrix.GetPermissions(
      [SystemRoles.Cashier, SystemRoles.InventoryManager]);

    // From Cashier
    permissions.Should().Contain(SystemPermissions.SalesCreate);
    // From InventoryManager
    permissions.Should().Contain(SystemPermissions.InventoryAdjust);
  }

  [Fact]
  public void AllSystemRolesShouldResolveToNonEmptyPermissions()
  {
    foreach (var role in SystemRoles.All)
    {
      var permissions = RolePermissionMatrix.GetPermissions(role);

      permissions.Should().NotBeEmpty($"role '{role}' should have at least one permission");
    }
  }

  [Fact]
  public void OwnerAndAdminShouldHaveIdenticalPermissions()
  {
    var ownerPermissions = RolePermissionMatrix.GetPermissions(SystemRoles.Owner);
    var adminPermissions = RolePermissionMatrix.GetPermissions(SystemRoles.Admin);

    ownerPermissions.Should().BeEquivalentTo(adminPermissions);
  }
}
