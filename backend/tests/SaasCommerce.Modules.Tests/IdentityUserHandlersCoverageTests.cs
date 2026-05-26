using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Application.Users;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Events.V1;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Modules.Tests;

public sealed class IdentityUserHandlersCoverageTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 17, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task CreateUser_ShouldCreateUserAuditAndEvent()
  {
    var scenario = IdentityScenario.Create();
    var handler = scenario.CreateUserHandler();

    var result = await handler.Handle(new CreateUserCommand(
      "  Nuevo Usuario  ",
      "Nuevo@Test.com",
      "Password123!",
      SystemRoles.Cashier,
      scenario.BranchId));

    result.IsSuccess.Should().BeTrue();
    scenario.Repository.Users.Should().ContainSingle(user => user.Id == result.Value && user.Email == "nuevo@test.com");
    scenario.Repository.Roles.Should().ContainSingle(role => role.Name == SystemRoles.Cashier);
    scenario.Audit.Entries.Should().ContainSingle(entry => entry.Action == "user.created");
    scenario.EventBus.Messages.Should().ContainSingle(message => message is UserCreatedIntegrationEventV1);
    scenario.UnitOfWork.SaveCount.Should().Be(1);
  }

  [Fact]
  public async Task CreateUser_ShouldReturnValidationFailures()
  {
    var scenario = IdentityScenario.Create();
    var handler = scenario.CreateUserHandler();

    scenario.CurrentUser.Authenticated = false;
    (await handler.Handle(ValidCreate())).Error.Should().Be(IdentityPermissionsErrors.UserContextRequired);

    scenario = IdentityScenario.Create();
    handler = scenario.CreateUserHandler();
    (await handler.Handle(ValidCreate() with { Email = "bad" })).Error.Should().Be(IdentityPermissionsErrors.InvalidEmail);
    (await handler.Handle(ValidCreate() with { Password = "short" })).Error.Should().Be(IdentityPermissionsErrors.PasswordTooShort);
    (await handler.Handle(ValidCreate() with { Role = SystemRoles.Owner })).Error.Should().Be(IdentityPermissionsErrors.CannotCreateOwnerRole);
    (await handler.Handle(ValidCreate() with { Role = "Unknown" })).Error.Should().Be(IdentityPermissionsErrors.InvalidRole);

    scenario.LimitChecker.Allowed = false;
    (await handler.Handle(ValidCreate())).Error.Code.Should().Be("subscription.limit_reached");

    scenario.LimitChecker.Allowed = true;
    scenario.Repository.DuplicateEmail = true;
    (await handler.Handle(ValidCreate())).Error.Should().Be(IdentityPermissionsErrors.DuplicateEmail);
  }

  [Fact]
  public async Task ResetUserPassword_ShouldUpdatePasswordForceChangeAuditAndEvent()
  {
    var scenario = IdentityScenario.Create();
    var target = scenario.AddExistingUser();
    var handler = scenario.ResetPasswordHandler();

    var result = await handler.Handle(new ResetUserPasswordCommand(target.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.TemporaryPassword.Should().HaveLength(12);
    target.PasswordHash.Should().StartWith("hashed:");
    target.MustChangePassword.Should().BeTrue();
    scenario.Audit.Entries.Should().ContainSingle(entry => entry.Action == "user.password_reset");
    scenario.EventBus.Messages.Should().ContainSingle(message => message is UserPasswordResetIntegrationEventV1);
    scenario.UnitOfWork.SaveCount.Should().Be(1);
  }

  [Fact]
  public async Task ResetUserPassword_ShouldReturnFailures()
  {
    var scenario = IdentityScenario.Create();
    var handler = scenario.ResetPasswordHandler();

    scenario.CurrentUser.Authenticated = false;
    (await handler.Handle(new ResetUserPasswordCommand(Guid.NewGuid()))).Error.Should().Be(IdentityPermissionsErrors.UserContextRequired);

    scenario = IdentityScenario.Create();
    handler = scenario.ResetPasswordHandler();
    (await handler.Handle(new ResetUserPasswordCommand(scenario.UserId))).Error.Should().Be(IdentityPermissionsErrors.CannotResetOwnPassword);
    (await handler.Handle(new ResetUserPasswordCommand(Guid.NewGuid()))).Error.Should().Be(IdentityPermissionsErrors.UserNotFound);
  }

  private static CreateUserCommand ValidCreate()
    => new("Nuevo Usuario", "nuevo@test.com", "Password123!", SystemRoles.Cashier, null);

  private sealed class IdentityScenario
  {
    public Guid BusinessId { get; } = Guid.NewGuid();
    public Guid UserId { get; } = Guid.NewGuid();
    public Guid BranchId { get; } = Guid.NewGuid();
    public TestUserManagementRepository Repository { get; } = new();
    public TestPasswordHasher PasswordHasher { get; } = new();
    public TestClock Clock { get; } = new();
    public TestUnitOfWork UnitOfWork { get; } = new();
    public TestAuditLogWriter Audit { get; } = new();
    public TestEventBus EventBus { get; } = new();
    public TestLimitChecker LimitChecker { get; } = new();
    public TestCurrentUser CurrentUser { get; private set; } = null!;

    public static IdentityScenario Create()
    {
      var scenario = new IdentityScenario();
      scenario.CurrentUser = new TestCurrentUser(scenario.BusinessId, scenario.UserId);
      return scenario;
    }

    public CreateUserHandler CreateUserHandler()
      => new(new CreateUserDependencies
      {
        Repository = Repository,
        CurrentUser = CurrentUser,
        PasswordHasher = PasswordHasher,
        Clock = Clock,
        UnitOfWork = UnitOfWork,
        AuditLogWriter = Audit,
        EventBus = EventBus,
        LimitChecker = LimitChecker
      });

    public ResetUserPasswordHandler ResetPasswordHandler()
      => new(Repository, CurrentUser, PasswordHasher, Clock, UnitOfWork, Audit, EventBus);

    public User AddExistingUser()
    {
      var user = new User(
        Guid.NewGuid(),
        new BusinessId(BusinessId),
        new BranchId(BranchId),
        "Target User",
        "target@test.com",
        "old-hash",
        Now);
      Repository.Users.Add(user);
      return user;
    }
  }

  private sealed class TestUserManagementRepository : IUserManagementRepository
  {
    public List<User> Users { get; } = [];
    public List<Role> Roles { get; } = [];
    public bool DuplicateEmail { get; set; }

    public Task CreateUserAsync(User user, Role role, CancellationToken cancellationToken = default)
    {
      user.AddRole(role);
      Users.Add(user);
      Roles.Add(role);
      return Task.CompletedTask;
    }

    public Task<bool> EmailExistsInBusinessAsync(string email, BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(DuplicateEmail || Users.Any(user => user.BusinessId == businessId && user.Email == email));

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
      => Task.FromResult(Users.SingleOrDefault(user => user.Id == userId));

    public Task<User?> GetByIdInBusinessAsync(Guid userId, BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Users.SingleOrDefault(user => user.Id == userId && user.BusinessId == businessId));

    public Task<UserListResponse> GetUsersAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(new UserListResponse([], 0));

    public Task<bool> HasOtherAdminOrOwnerAsync(Guid excludeUserId, BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(true);
  }

  private sealed class TestPasswordHasher : IPasswordHasher
  {
    public string Hash(string password) => $"hashed:{password}";
    public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
  }

  private sealed class TestCurrentUser(Guid businessId, Guid userId) : ICurrentUserService
  {
    public bool Authenticated { get; set; } = true;
    public Guid? UserId => userId;
    public Guid? BusinessId => Authenticated ? businessId : null;
    public Guid? BranchId => null;
    public IReadOnlyCollection<string> Roles => [SystemRoles.Admin];
    public bool IsAuthenticated => Authenticated;
  }

  private sealed class TestClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class TestUnitOfWork : IUnitOfWork
  {
    public int SaveCount { get; private set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveCount++;
      return Task.FromResult(1);
    }
  }

  private sealed class TestAuditLogWriter : IAuditLogWriter
  {
    public List<AuditEntry> Entries { get; } = [];
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
      Entries.Add(entry);
      return Task.CompletedTask;
    }
  }

  private sealed class TestEventBus : IEventBus
  {
    public List<object> Messages { get; } = [];
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
      where TMessage : class
    {
      Messages.Add(message);
      return Task.CompletedTask;
    }

    public Task PublishAsync(object message, Type messageType, CancellationToken cancellationToken = default)
    {
      Messages.Add(message);
      return Task.CompletedTask;
    }
  }

  private sealed class TestLimitChecker : ISubscriptionLimitChecker
  {
    public bool Allowed { get; set; } = true;

    public Task<SubscriptionLimitCheckResult> CanCreateUserAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(new SubscriptionLimitCheckResult(Allowed, Allowed ? "OK" : "LIMIT", Allowed ? "Allowed" : "Too many users", 0, 1));

    public Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanCreateProductAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanCreateSaleAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanUseInventoryTransfersAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanUseAdvancedReportsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
    public Task<SubscriptionLimitCheckResult> CanUseAuditLogsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => throw new NotSupportedException();
  }
}

#pragma warning restore CA1707
