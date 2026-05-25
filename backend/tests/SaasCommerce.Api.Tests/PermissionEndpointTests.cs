#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Api.Tests;

/// <summary>
/// Validates that role-based permission enforcement is working correctly.
/// Cashier role must be blocked from admin/export operations.
/// Admin role must have full access.
/// </summary>
public sealed class PermissionEndpointTests
{
  private const string CashierEmail = "cajero@test.com";
  private const string CashierPassword = "Cajero123!";
  private const string InventoryManagerEmail = "inventario@test.com";
  private const string InventoryManagerPassword = "Inventario123!";

  // Known seeded IDs from DevelopmentDataSeeder
  private static readonly Guid SeedBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
  private static readonly Guid SeedBranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");

  // ── Pilot Metrics (Admin only) ────────────────────────────────────────────

  [Fact]
  public async Task GetPilotMetrics_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/admin/pilot-metrics");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task GetPilotMetrics_ShouldReturn200_WhenAuthenticatedAsAdmin()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsAdminAsync(client);

    var response = await client.GetAsync("/api/admin/pilot-metrics");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  // ── Export endpoints (require specific export permissions) ─────────────────

  [Fact]
  public async Task ProductsExport_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/products/export");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task SalesExport_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/sales/export");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task CustomersExport_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/customers/export");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task InventoryExport_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/inventory/export");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task CashExport_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/cash-registers/export");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  // ── Inventory management (requires InventoryAdjust permission) ─────────────

  [Fact]
  public async Task InventoryAdjust_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/inventory/adjustments",
      new { productId = Guid.NewGuid(), quantityChange = 1, reason = "Test" });

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  // ── User management (Admin only) ──────────────────────────────────────────

  [Fact]
  public async Task CreateUser_ShouldReturn403_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/users",
      new CreateUserRequest("Extra User", "extra@test.com", "Test123!", "Cashier", null));

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  // ── Cashier CAN access these endpoints ───────────────────────────────────

  [Fact]
  public async Task GetSales_ShouldReturn200_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/sales");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetProducts_ShouldReturn200_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/catalog/products");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task GetCustomers_ShouldReturn200_WhenAuthenticatedAsCashier()
  {
    using var factory = CreateFactory();
    await SeedCashierAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsCashierAsync(client);

    var response = await client.GetAsync("/api/customers");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  // ── InventoryManager role ─────────────────────────────────────────────────

  [Fact]
  public async Task InventoryExport_ShouldReturn403_WhenAuthenticatedAsInventoryManager()
  {
    using var factory = CreateFactory();
    await SeedInventoryManagerAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsInventoryManagerAsync(client);

    // InventoryManager does not have InventoryExport permission
    var response = await client.GetAsync("/api/inventory/export");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task GetInventory_ShouldReturn200_WhenAuthenticatedAsInventoryManager()
  {
    using var factory = CreateFactory();
    await SeedInventoryManagerAsync(factory);
    using var client = factory.CreateClient();
    await AuthenticateAsInventoryManagerAsync(client);

    var response = await client.GetAsync("/api/inventory");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  /// <summary>
  /// Seeds a Cashier user directly into the in-memory database, bypassing the API.
  /// This avoids the complexity of user-creation logic (audit, events, limit-checks).
  /// </summary>
  private static async Task SeedCashierAsync(WebApplicationFactory<Program> factory)
    => await SeedUserAsync(factory, CashierEmail, CashierPassword, SystemRoles.Cashier);

  private static async Task SeedInventoryManagerAsync(WebApplicationFactory<Program> factory)
    => await SeedUserAsync(factory, InventoryManagerEmail, InventoryManagerPassword, SystemRoles.InventoryManager);

  private static async Task SeedUserAsync(
    WebApplicationFactory<Program> factory,
    string email,
    string password,
    string roleName)
  {
    // Force startup (factory is lazy)
    _ = factory.Server;

    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    var businessId = new BusinessId(SeedBusinessId);
    var branchId = new BranchId(SeedBranchId);

    var userId = Guid.NewGuid();
    var hashedPassword = passwordHasher.Hash(password);
    var now = DateTimeOffset.UtcNow;

    var user = new User(userId, businessId, branchId, "Test User", email, hashedPassword, now);
    var role = new Role(Guid.NewGuid(), businessId, roleName);
    user.AddRole(role);

    dbContext.Set<User>().Add(user);
    await dbContext.SaveChangesAsync(CancellationToken.None);
  }

  private static async Task AuthenticateAsAdminAsync(HttpClient client)
  {
    var response = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", login!.Data!.AccessToken);
  }

  private static async Task AuthenticateAsCashierAsync(HttpClient client)
  {
    var response = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest(CashierEmail, CashierPassword));
    var login = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", login!.Data!.AccessToken);
  }

  private static async Task AuthenticateAsInventoryManagerAsync(HttpClient client)
  {
    var response = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest(InventoryManagerEmail, InventoryManagerPassword));
    var login = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", login!.Data!.AccessToken);
  }

  private static WebApplicationFactory<Program> CreateFactory()
    => new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
          cfg.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionStrings:DefaultConnection"] = "",
            ["Database:InMemoryName"] = Guid.NewGuid().ToString("D"),
            ["RabbitMq:UseInMemory"] = "true",
            ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
            ["Jwt:Issuer"] = "SaasCommerce.Tests",
            ["Jwt:Audience"] = "SaasCommerce.Tests",
            ["Jwt:AccessTokenMinutes"] = "30"
          });
        });
      });
}

#pragma warning restore CA1707
