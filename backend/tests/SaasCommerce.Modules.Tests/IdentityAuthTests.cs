using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class IdentityAuthTests
{
  [Fact]
  public async Task LoginHandlerShouldReturnTokensForValidCredentials()
  {
    await using var dbContext = CreateDbContext();
    var clock = new FixedClock(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
    var passwordHasher = new PasswordHasher();
    var user = await SeedUserAsync(dbContext, passwordHasher, clock);
    var handler = CreateLoginHandler(dbContext, passwordHasher, clock);

    var result = await handler.Handle(new LoginCommand(user.Email, "Admin123!"));

    result.IsSuccess.Should().BeTrue();
    result.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
    result.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();
    result.Value.User.Id.Should().Be(user.Id);
    result.Value.User.BusinessId.Should().Be(user.BusinessId.Value);
    result.Value.User.BranchId.Should().Be(user.DefaultBranchId?.Value);
    result.Value.User.Roles.Should().ContainSingle().Which.Should().Be("Admin");
  }

  [Fact]
  public async Task LoginHandlerShouldRejectInvalidPassword()
  {
    await using var dbContext = CreateDbContext();
    var clock = new FixedClock(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
    var passwordHasher = new PasswordHasher();
    var user = await SeedUserAsync(dbContext, passwordHasher, clock);
    var handler = CreateLoginHandler(dbContext, passwordHasher, clock);

    var result = await handler.Handle(new LoginCommand(user.Email, "wrong-password"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(IdentityErrors.InvalidCredentials);
  }

  [Fact]
  public async Task JwtTokenShouldIncludeBusinessBranchUserAndRoleClaims()
  {
    await using var dbContext = CreateDbContext();
    var clock = new FixedClock(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
    var passwordHasher = new PasswordHasher();
    var user = await SeedUserAsync(dbContext, passwordHasher, clock);
    var jwtTokenService = new JwtTokenService(CreateConfiguration(), clock);

    var token = jwtTokenService.CreateAccessToken(user);
    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);
    var expectedBranchId = user.DefaultBranchId!.Value.Value.ToString("D");

    jwt.Claims.Should().Contain(claim => claim.Type == "business_id" && claim.Value == user.BusinessId.Value.ToString("D"));
    jwt.Claims.Should().Contain(claim => claim.Type == "branch_id" && claim.Value == expectedBranchId);
    jwt.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString("D"));
    jwt.Claims.Should().Contain(claim => claim.Type == System.Security.Claims.ClaimTypes.Role && claim.Value == "Admin");
  }

  [Fact]
  public async Task RefreshTokenHandlerShouldRotateActiveRefreshToken()
  {
    await using var dbContext = CreateDbContext();
    var clock = new FixedClock(new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero));
    var passwordHasher = new PasswordHasher();
    var user = await SeedUserAsync(dbContext, passwordHasher, clock);
    var loginHandler = CreateLoginHandler(dbContext, passwordHasher, clock);
    var login = await loginHandler.Handle(new LoginCommand(user.Email, "Admin123!"));
    var refreshHandler = CreateRefreshHandler(dbContext, clock);

    var refreshed = await refreshHandler.Handle(new RefreshTokenCommand(login.Value.RefreshToken));

    refreshed.IsSuccess.Should().BeTrue();
    refreshed.Value.RefreshToken.Should().NotBe(login.Value.RefreshToken);
    refreshed.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
  }

  private static LoginHandler CreateLoginHandler(
    AppDbContext dbContext,
    PasswordHasher passwordHasher,
    IClock clock)
    => new(
      new EfIdentityUserRepository(dbContext),
      passwordHasher,
      new JwtTokenService(CreateConfiguration(), clock),
      new RefreshTokenGenerator(),
      clock,
      new EfUnitOfWork(dbContext));

  private static RefreshTokenHandler CreateRefreshHandler(
    AppDbContext dbContext,
    IClock clock)
    => new(
      new EfIdentityUserRepository(dbContext),
      new JwtTokenService(CreateConfiguration(), clock),
      new RefreshTokenGenerator(),
      clock,
      new EfUnitOfWork(dbContext));

  private static async Task<User> SeedUserAsync(
    AppDbContext dbContext,
    PasswordHasher passwordHasher,
    IClock clock)
  {
    var businessId = BusinessId.New();
    var branchId = BranchId.New();
    var business = new Business(businessId, "Test Business", clock.UtcNow);
    var role = new Role(Guid.NewGuid(), businessId, "Admin");
    var user = new User(
      Guid.NewGuid(),
      businessId,
      branchId,
      "Admin",
      "admin@test.com",
      passwordHasher.Hash("Admin123!"),
      clock.UtcNow);

    business.AddBranch(branchId, "Main", "MAIN", clock.UtcNow, isMain: true);
    user.AddRole(role);
    dbContext.Add(business);
    dbContext.Add(role);
    dbContext.Add(user);

    await dbContext.SaveChangesAsync();

    return user;
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

  private sealed class FixedClock(DateTimeOffset utcNow) : IClock
  {
    public DateTimeOffset UtcNow { get; } = utcNow;
  }
}
