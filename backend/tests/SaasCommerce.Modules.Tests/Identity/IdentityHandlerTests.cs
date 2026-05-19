#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Audit;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Application.Users;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests.Identity;

public sealed class IdentityHandlerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  // ── DisableUserHandler ──────────────────────────────────────────────────

  [Fact]
  public async Task DisableUser_ShouldDeactivateUser_WhenUserExists()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var user = MakeUser(businessId);
    var repo = new StubUserRepo(user);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new DisableUserHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new DisableUserCommand(user.Id));

    result.IsSuccess.Should().BeTrue();
    user.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task DisableUser_ShouldFail_WhenTargetUserIsSelf()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var userId = Guid.NewGuid();
    var user = MakeUser(businessId, userId);
    var repo = new StubUserRepo(user);
    var currentUser = MakeCurrentUser(businessId.Value, userId);
    var handler = new DisableUserHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new DisableUserCommand(userId));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.CannotDisableSelf);
  }

  [Fact]
  public async Task DisableUser_ShouldFail_WhenUserNotFound()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var repo = new StubUserRepo(null);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new DisableUserHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new DisableUserCommand(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.UserNotFound);
  }

  [Fact]
  public async Task DisableUser_ShouldFail_WhenNotAuthenticated()
  {
    var repo = new StubUserRepo(null);
    var currentUser = new UnauthenticatedUser();
    var handler = new DisableUserHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new DisableUserCommand(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.UserContextRequired);
  }

  // ── UpdateUserRoleHandler ───────────────────────────────────────────────

  [Fact]
  public async Task UpdateUserRole_ShouldChangeRole_WhenValid()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var user = MakeUser(businessId);
    var repo = new StubUserRepo(user);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new UpdateUserRoleHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new UpdateUserRoleCommand(user.Id, SystemRoles.Cashier));

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateUserRole_ShouldFail_WhenRoleIsInvalid()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var user = MakeUser(businessId);
    var repo = new StubUserRepo(user);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new UpdateUserRoleHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new UpdateUserRoleCommand(user.Id, "SuperAdmin"));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("identity.invalid_role");
  }

  [Fact]
  public async Task UpdateUserRole_ShouldFail_WhenUserNotFound()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var repo = new StubUserRepo(null);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new UpdateUserRoleHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new UpdateUserRoleCommand(Guid.NewGuid(), SystemRoles.Cashier));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.UserNotFound);
  }

  [Fact]
  public async Task UpdateUserRole_ShouldFail_WhenLastOwnerDemoted()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var user = MakeUser(businessId, role: SystemRoles.Owner);
    var repo = new StubUserRepo(user, hasOtherAdmin: false);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new UpdateUserRoleHandler(repo, currentUser, new FixedClock(), new NoopUnitOfWork());

    var result = await handler.Handle(new UpdateUserRoleCommand(user.Id, SystemRoles.Cashier));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.CannotRemoveLastOwner);
  }

  // ── GetCurrentUserPermissionsHandler ───────────────────────────────────

  [Fact]
  public void GetPermissions_ShouldReturnPermissions_WhenAuthenticated()
  {
    var businessId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var currentUser = MakeCurrentUser(businessId, userId, roles: [SystemRoles.Admin]);
    var permissionService = new PermissionService();
    var handler = new GetCurrentUserPermissionsHandler(currentUser, permissionService);

    var result = handler.Handle(new GetCurrentUserPermissionsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.UserId.Should().Be(userId);
    result.Value.BusinessId.Should().Be(businessId);
    result.Value.Role.Should().Be(SystemRoles.Admin);
    result.Value.Permissions.Should().NotBeEmpty();
  }

  [Fact]
  public void GetPermissions_ShouldFail_WhenNotAuthenticated()
  {
    var currentUser = new UnauthenticatedUser();
    var permissionService = new PermissionService();
    var handler = new GetCurrentUserPermissionsHandler(currentUser, permissionService);

    var result = handler.Handle(new GetCurrentUserPermissionsQuery());

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.UserContextRequired);
  }

  // ── GetAuditLogsHandler ─────────────────────────────────────────────────

  [Fact]
  public async Task GetAuditLogs_ShouldReturnLogs_WhenAuthenticated()
  {
    var businessId = Guid.NewGuid();
    var currentUser = MakeCurrentUser(businessId, Guid.NewGuid());
    var repo = new StubAuditLogRepo();
    var handler = new GetAuditLogsHandler(repo, currentUser);

    var result = await handler.Handle(new GetAuditLogsQuery(null, null, null, null, 1, 20));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task GetAuditLogs_ShouldFail_WhenNotAuthenticated()
  {
    var currentUser = new UnauthenticatedUser();
    var repo = new StubAuditLogRepo();
    var handler = new GetAuditLogsHandler(repo, currentUser);

    var result = await handler.Handle(new GetAuditLogsQuery(null, null, null, null, 1, 20));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetAuditLogs_ShouldClampPagination()
  {
    var businessId = Guid.NewGuid();
    var currentUser = MakeCurrentUser(businessId, Guid.NewGuid());
    var repo = new StubAuditLogRepo();
    var handler = new GetAuditLogsHandler(repo, currentUser);

    // Page 0 and pageSize 0 should be clamped to 1 and 1
    var result = await handler.Handle(new GetAuditLogsQuery(null, null, null, null, 0, 0));

    result.IsSuccess.Should().BeTrue();
    repo.LastPage.Should().Be(1);
    repo.LastPageSize.Should().Be(1);
  }

  // ── GetUsersHandler ─────────────────────────────────────────────────────

  [Fact]
  public async Task GetUsers_ShouldReturnUserList_WhenAuthenticated()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var repo = new StubUserRepo(null);
    var currentUser = MakeCurrentUser(businessId.Value, Guid.NewGuid());
    var handler = new GetUsersHandler(repo, currentUser);

    var result = await handler.Handle(new GetUsersQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task GetUsers_ShouldFail_WhenNotAuthenticated()
  {
    var repo = new StubUserRepo(null);
    var currentUser = new UnauthenticatedUser();
    var handler = new GetUsersHandler(repo, currentUser);

    var result = await handler.Handle(new GetUsersQuery());

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityPermissionsErrors.UserContextRequired);
  }

  // ── PermissionAuthorizationHandler ─────────────────────────────────────

  [Fact]
  public async Task PermissionAuthorizationHandler_ShouldSucceed_WhenUserHasPermission()
  {
    var handler = new PermissionAuthorizationHandler();
    var requirement = new PermissionRequirement(SystemPermissions.DashboardView);
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
      [new Claim(ClaimTypes.Role, SystemRoles.Admin)], "Test"));
    var context = new AuthorizationHandlerContext([requirement], principal, null);

    await handler.HandleAsync(context);

    context.HasSucceeded.Should().BeTrue();
  }

  [Fact]
  public async Task PermissionAuthorizationHandler_ShouldNotSucceed_WhenUserLacksPermission()
  {
    var handler = new PermissionAuthorizationHandler();
    var requirement = new PermissionRequirement(SystemPermissions.DashboardView);
    var principal = new ClaimsPrincipal(new ClaimsIdentity([], "Test"));
    var context = new AuthorizationHandlerContext([requirement], principal, null);

    await handler.HandleAsync(context);

    context.HasSucceeded.Should().BeFalse();
  }

  // ── PermissionService ───────────────────────────────────────────────────

  [Fact]
  public void PermissionService_ShouldReturnPermissionsForRole()
  {
    var service = new PermissionService();

    var permissions = service.GetPermissionsForRoles([SystemRoles.Admin]);

    permissions.Should().NotBeEmpty();
    permissions.Should().Contain(SystemPermissions.UsersView);
  }

  [Fact]
  public void PermissionService_HasPermission_ShouldReturnTrue_WhenRoleHasIt()
  {
    var service = new PermissionService();

    var has = service.HasPermission([SystemRoles.Admin], SystemPermissions.DashboardView);

    has.Should().BeTrue();
  }

  // ── Helpers ─────────────────────────────────────────────────────────────

  private static User MakeUser(
    BusinessId businessId,
    Guid? userId = null,
    string role = SystemRoles.Admin)
  {
    var user = new User(
      userId ?? Guid.NewGuid(),
      businessId,
      null,
      "Test User",
      "test@example.com",
      "hash",
      Now);

    var roleEntity = new Role(Guid.NewGuid(), businessId, role);
    user.AddRole(roleEntity);
    return user;
  }

  private static FakeCurrentUser MakeCurrentUser(
    Guid businessId,
    Guid userId,
    IReadOnlyCollection<string>? roles = null)
    => new(businessId, userId, roles ?? [SystemRoles.Admin]);

  // ── Stubs ────────────────────────────────────────────────────────────────

  private sealed class StubUserRepo(User? user, bool hasOtherAdmin = true) : IUserManagementRepository
  {
    public Task<UserListResponse> GetUsersAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(new UserListResponse([], 0));

    public Task<User?> GetByIdInBusinessAsync(
      Guid userId,
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(user?.Id == userId ? user : null);

    public Task<bool> HasOtherAdminOrOwnerAsync(
      Guid excludeUserId,
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(hasOtherAdmin);
  }

  private sealed class StubAuditLogRepo : IAuditLogReadRepository
  {
    public int LastPage { get; private set; }
    public int LastPageSize { get; private set; }

    public Task<AuditLogListResponse> GetAuditLogsAsync(
      BusinessId businessId,
      AuditLogCriteria criteria,
      CancellationToken cancellationToken = default)
    {
      LastPage = criteria.Page;
      LastPageSize = criteria.PageSize;
      return Task.FromResult(new AuditLogListResponse([], criteria.Page, criteria.PageSize, 0, 0, false, false));
    }
  }

  private sealed class FakeCurrentUser(Guid businessId, Guid userId, IReadOnlyCollection<string> roles) : ICurrentUserService
  {
    public Guid? UserId => userId;
    public Guid? BusinessId => businessId;
    public Guid? BranchId => Guid.NewGuid();
    public IReadOnlyCollection<string> Roles => roles;
    public bool IsAuthenticated => true;
  }

  private sealed class UnauthenticatedUser : ICurrentUserService
  {
    public Guid? UserId => null;
    public Guid? BusinessId => null;
    public Guid? BranchId => null;
    public IReadOnlyCollection<string> Roles => [];
    public bool IsAuthenticated => false;
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUnitOfWork : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.FromResult(1);
  }
}

#pragma warning restore CA1707
