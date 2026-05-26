using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.PilotBusiness;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.Modules.Tenancy.Infrastructure.Persistence;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class PilotBusinessTests
{
  private static readonly DateTimeOffset FixedNow =
    new(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task CreatePilotBusinessHandlerShouldCreateBusinessBranchAndAdminWhenRequestIsValid()
  {
    await using var db = CreateDbContext();
    await SeedPlansAsync(db);
    var handler = CreateHandler(db);

    var cmd = ValidCommand();
    var result = await handler.Handle(cmd);

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessName.Should().Be(cmd.BusinessName);
    result.Value.AdminEmail.Should().Be(cmd.AdminEmail);
    result.Value.TrialEndsAt.Should().NotBeNull();
    result.Value.BusinessId.Should().NotBe(Guid.Empty);
    result.Value.BranchId.Should().NotBe(Guid.Empty);
    result.Value.AdminUserId.Should().NotBe(Guid.Empty);

    // Verify entities persisted
    var businessExists = await db.Set<Business>()
      .AnyAsync(b => b.Id == new BusinessId(result.Value.BusinessId));
    businessExists.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePilotBusinessHandlerShouldRejectDuplicateEmailWhenAdminEmailAlreadyExists()
  {
    await using var db = CreateDbContext();
    await SeedPlansAsync(db);
    var handler = CreateHandler(db);

    // Create first business
    await handler.Handle(ValidCommand("business1@test.com", "132001235"));

    // Try to create second with same email
    var result = await handler.Handle(ValidCommand("business1@test.com", "132001236"));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("PILOT_BUSINESS_DUPLICATE_EMAIL");
  }

  [Fact]
  public async Task CreatePilotBusinessHandlerShouldRejectDuplicateIdentificationWhenRncAlreadyExists()
  {
    await using var db = CreateDbContext();
    await SeedPlansAsync(db);
    var handler = CreateHandler(db);

    await handler.Handle(ValidCommand("biz1@test.com", "132001234"));
    var result = await handler.Handle(ValidCommand("biz2@test.com", "132001234")); // same RNC

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("PILOT_BUSINESS_DUPLICATE_IDENTIFICATION");
  }

  [Fact]
  public async Task CreatePilotBusinessHandlerShouldFailValidationWhenRequiredFieldsMissing()
  {
    await using var db = CreateDbContext();
    await SeedPlansAsync(db);
    var handler = CreateHandler(db);

    var cmd = new CreatePilotBusinessCommand(
      string.Empty, "Rnc", "132001234", "8091234567",
      string.Empty, string.Empty, string.Empty, "short");

    var result = await handler.Handle(cmd);

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("VALIDATION_ERROR");
  }

  [Fact]
  public async Task CreatePilotBusinessHandlerShouldRejectPasswordTooShort()
  {
    await using var db = CreateDbContext();
    await SeedPlansAsync(db);
    var handler = CreateHandler(db);

    var cmd = ValidCommand() with { AdminPassword = "short" };
    var result = await handler.Handle(cmd);

    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreatePilotBusinessHandlerShouldRejectInvalidIdentificationType()
  {
    await using var db = CreateDbContext();
    await SeedPlansAsync(db);
    var handler = CreateHandler(db);

    var cmd = ValidCommand() with { IdentificationType = "INVALID" };
    var result = await handler.Handle(cmd);

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("PILOT_BUSINESS_INVALID_IDENTIFICATION_TYPE");
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static CreatePilotBusinessCommand ValidCommand(
    string email = "pilot@test.com",
    string rnc = "132001234") => new(
      "Colmado El Piloto SRL",
      "Rnc",
      rnc,
      "8091234567",
      "Sucursal Principal",
      "Juan Pérez",
      email,
      "Admin123!");

  private static CreatePilotBusinessHandler CreateHandler(AppDbContext db)
  {
    var clock = new FixedClock(FixedNow);
    var passwordHasher = new PasswordHasher();
    var businesses = new EfAccountBusinessRepository(db);
    var users = new EfIdentityUserRepository(db);
    var subscriptionPlans = new EfSubscriptionPlanRepository(db);
    var subscriptions = new EfBusinessSubscriptionRepository(db);
    var unitOfWork = new EfUnitOfWork(db);
    var currentUser = new FakeSaasAdminCurrentUser();
    var auditLogWriter = new NoOpAuditLogWriter();

    return new CreatePilotBusinessHandler(new CreatePilotBusinessDependencies
    {
      Businesses = businesses,
      Users = users,
      PasswordHasher = passwordHasher,
      SubscriptionPlans = subscriptionPlans,
      Subscriptions = subscriptions,
      Clock = clock,
      UnitOfWork = unitOfWork,
      AuditLogWriter = auditLogWriter,
      CurrentUser = currentUser
    });
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static async Task SeedPlansAsync(AppDbContext db)
  {
    await SaasCommerce.Modules.Development.BillingDataSeeder
      .SeedPlansAsync(db, FixedNow);
  }

  // ── Fakes ─────────────────────────────────────────────────────────────────

  private sealed class FakeSaasAdminCurrentUser : ICurrentUserService
  {
    public Guid? UserId => Guid.Parse("44444444-4444-4444-4444-444444444444");
    public Guid? BusinessId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid? BranchId => Guid.Parse("22222222-2222-2222-2222-222222222222");
    public IReadOnlyCollection<string> Roles => ["Admin"];
    public bool IsAuthenticated => true;
  }

  private sealed class NoOpAuditLogWriter : IAuditLogWriter
  {
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class EfUnitOfWork(AppDbContext db)
    : SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => db.SaveChangesAsync(cancellationToken);
  }

  private sealed class FixedClock(DateTimeOffset utcNow) : IClock
  {
    public DateTimeOffset UtcNow => utcNow;
  }
}
