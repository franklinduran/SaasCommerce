using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Infrastructure.Persistence;
using SaasCommerce.Modules.Development;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Tests;

public sealed class AccountRegistrationTests
{
  [Fact]
  public async Task RegisterBusinessShouldCreateBusinessBranchAdminAndTokens()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(Command(
      "Colmado La Fe",
      "admin@lafe.com",
      "Cedula",
      "00112345678",
      ["8090000000", "8290000000"]));

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().NotBeEmpty();
    result.Value.BranchId.Should().NotBeEmpty();
    result.Value.UserId.Should().NotBeEmpty();
    result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
    result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
    result.Value.User.Roles.Should().Contain("Admin");

    var business = await dbContext.Set<Business>()
      .Include(current => current.Branches)
      .Include(current => current.Phones)
      .SingleAsync(current => current.Id.Value == result.Value.BusinessId);

    business.Name.Should().Be("Colmado La Fe");
    business.IdentificationType.Should().Be(BusinessIdentificationType.Cedula);
    business.IdentificationNumber.Should().Be("00112345678");
    business.Branches.Should().ContainSingle(branch => branch.IsMain);
    business.Phones.Should().HaveCount(2);
    business.Phones.Should().ContainSingle(phone => phone.IsPrimary);
  }

  [Fact]
  public async Task RegisterBusinessShouldRejectDuplicateEmail()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    await handler.Handle(Command("Colmado Uno", "admin@test.com", "Rnc", "101123456"));
    var duplicate = await handler.Handle(Command("Colmado Dos", "admin@test.com", "Rnc", "101123457"));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(AccountErrors.DuplicateEmail);
  }

  [Fact]
  public async Task RegisterBusinessShouldRejectDuplicateIdentification()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    await handler.Handle(Command("Colmado Uno", "uno@test.com", "Passport", "A0001"));
    var duplicate = await handler.Handle(Command("Colmado Dos", "dos@test.com", "Passport", "A0001"));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(AccountErrors.DuplicateIdentification);
  }

  [Theory]
  [InlineData("", "Cedula", "00112345678", "Admin Principal", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "", "00112345678", "Admin Principal", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Cedula", "", "Admin Principal", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Cedula", "123", "Admin Principal", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Rnc", "123", "Admin Principal", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Passport", "A1", "Admin Principal", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Cedula", "00112345678", "", "admin@test.com", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Cedula", "00112345678", "Admin Principal", "invalid-email", "Password123!", "Sucursal principal")]
  [InlineData("Colmado", "Cedula", "00112345678", "Admin Principal", "admin@test.com", "", "Sucursal principal")]
  [InlineData("Colmado", "Cedula", "00112345678", "Admin Principal", "admin@test.com", "Password123!", "")]
  public async Task RegisterBusinessShouldRejectInvalidRequiredFields(
    string businessName,
    string identificationType,
    string identificationNumber,
    string ownerFullName,
    string email,
    string password,
    string branchName)
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(new RegisterBusinessCommand(
      businessName,
      ownerFullName,
      email,
      password,
      identificationType,
      identificationNumber,
      [new RegisterBusinessPhoneCommand("8090000000", "Principal", true)],
      branchName,
      BillingDataSeeder.BasicPlanId));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(AccountErrors.InvalidRegistration.Code);
  }

  [Fact]
  public async Task RegisterBusinessShouldRejectEmptyPhones()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(Command(
      "Colmado",
      "admin@test.com",
      "Cedula",
      "00112345678",
      []));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(AccountErrors.InvalidRegistration.Code);
  }

  [Fact]
  public async Task RegisterBusinessShouldRejectPhonesWithoutPrimary()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(new RegisterBusinessCommand(
      "Colmado",
      "Admin Principal",
      "admin@test.com",
      "Password123!",
      "Cedula",
      "00112345678",
      [new RegisterBusinessPhoneCommand("8090000000", "Local", false)],
      "Sucursal principal",
      BillingDataSeeder.BasicPlanId));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(AccountErrors.InvalidRegistration.Code);
  }

  [Fact]
  public async Task RegisterBusinessShouldRejectMoreThanOnePrimaryPhone()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(new RegisterBusinessCommand(
      "Colmado",
      "Admin Principal",
      "admin@test.com",
      "Password123!",
      "Cedula",
      "00112345678",
      [
        new RegisterBusinessPhoneCommand("8090000000", "Uno", true),
        new RegisterBusinessPhoneCommand("8290000000", "Dos", true)
      ],
      "Sucursal principal",
      BillingDataSeeder.BasicPlanId));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(AccountErrors.InvalidRegistration.Code);
    result.Error.Details.Should().Contain(error =>
      error.Code == "phones" &&
      error.Message == "Exactly one primary phone is required.");
  }

  [Theory]
  [InlineData("")]
  [InlineData("809-000-0000")]
  public async Task RegisterBusinessShouldRejectEmptyOrDuplicatePhones(string secondPhone)
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(new RegisterBusinessCommand(
      "Colmado",
      "Admin Principal",
      "admin@test.com",
      "Password123!",
      "Cedula",
      "00112345678",
      [
        new RegisterBusinessPhoneCommand("8090000000", "Uno", true),
        new RegisterBusinessPhoneCommand(secondPhone, "Dos", false)
      ],
      "Sucursal principal",
      BillingDataSeeder.BasicPlanId));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(AccountErrors.InvalidRegistration.Code);
    result.Error.Details.Should().Contain(error => error.Code == "phones");
  }

  [Fact]
  public async Task RegisterBusinessShouldRejectMissingPlan()
  {
    await using var dbContext = CreateDbContext();
    var handler = CreateHandler(dbContext);

    var result = await handler.Handle(new RegisterBusinessCommand(
      "Colmado",
      "Admin Principal",
      "admin@test.com",
      "Password123!",
      "Cedula",
      "00112345678",
      [new RegisterBusinessPhoneCommand("8090000000", "Principal", true)],
      "Sucursal principal"));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(AccountErrors.InvalidRegistration.Code);
    result.Error.Details.Should().Contain(error => error.Code == "planId");
  }

  private static RegisterBusinessCommand Command(
    string businessName,
    string email,
    string? identificationType,
    string? identificationNumber,
    IReadOnlyCollection<string>? phones = null)
    => new(
      businessName,
      "Admin Principal",
      email,
      "Password123!",
      identificationType,
      identificationNumber,
      (phones ?? ["8090000000"])
        .Select((phone, index) => new RegisterBusinessPhoneCommand(
          phone,
          index == 0 ? "Principal" : "Secundario",
          index == 0))
        .ToArray(),
      "Sucursal principal",
      BillingDataSeeder.BasicPlanId);

  private static RegisterBusinessHandler CreateHandler(AppDbContext dbContext)
  {
    var clock = new FixedClock();
    BillingDataSeeder.SeedPlansAsync(dbContext, clock.UtcNow).GetAwaiter().GetResult();

    return new RegisterBusinessHandler(new RegisterBusinessDependencies
    {
      Businesses = new EfAccountBusinessRepository(dbContext),
      Users = new EfIdentityUserRepository(dbContext),
      PasswordHasher = new PasswordHasher(),
      JwtTokenService = new JwtTokenService(CreateConfiguration(), clock),
      RefreshTokenGenerator = new RefreshTokenGenerator(),
      SubscriptionPlans = new EfSubscriptionPlanRepository(dbContext),
      Subscriptions = new EfBusinessSubscriptionRepository(dbContext),
      Clock = clock,
      UnitOfWork = new EfUnitOfWork(dbContext)
    });
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static IConfiguration CreateConfiguration()
    => new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
        ["Jwt:Issuer"] = "SaasCommerce.Tests",
        ["Jwt:Audience"] = "SaasCommerce.Tests",
        ["Jwt:AccessTokenMinutes"] = "30"
      })
      .Build();

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow { get; } =
      new(2026, 5, 15, 12, 0, 0, TimeSpan.Zero);
  }
}
