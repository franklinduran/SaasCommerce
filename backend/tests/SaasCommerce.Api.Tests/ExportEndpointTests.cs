using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;

namespace SaasCommerce.Api.Tests;

public sealed class ExportEndpointTests
{
  // ── Products export ───────────────────────────────────────────────────────

  [Fact]
  public async Task GetProductsExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/products/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetProductsExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/products/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("SKU");
    body.Should().Contain("Nombre");
  }

  // ── Sales export ──────────────────────────────────────────────────────────

  [Fact]
  public async Task GetSalesExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/sales/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetSalesExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/sales/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("Código");
  }

  // ── Inventory export ──────────────────────────────────────────────────────

  [Fact]
  public async Task GetInventoryExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/inventory/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetInventoryExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/inventory/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("SKU");
    body.Should().Contain("Producto");
  }

  // ── Customers export ──────────────────────────────────────────────────────

  [Fact]
  public async Task GetCustomersExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/customers/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCustomersExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/customers/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("Nombre");
  }

  // ── Customer credits export ───────────────────────────────────────────────

  [Fact]
  public async Task GetCustomerCreditsExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/customer-credits/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCustomerCreditsExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/customer-credits/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("Cliente");
  }

  // ── Cash registers export ─────────────────────────────────────────────────

  [Fact]
  public async Task GetCashRegistersExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/cash-registers/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetCashRegistersExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/cash-registers/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("Fecha Apertura");
  }

  // ── Daily closings export ─────────────────────────────────────────────────

  [Fact]
  public async Task GetDailyClosingsExport_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/daily/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetDailyClosingsExport_ShouldReturn200WithCsv_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/reports/daily/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("Fecha");
  }

  // ── Pilot Metrics ─────────────────────────────────────────────────────────

  [Fact]
  public async Task GetPilotMetrics_ShouldReturn401_WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/admin/pilot-metrics");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetPilotMetrics_ShouldReturn200_WhenAuthenticatedAsAdmin()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/admin/pilot-metrics");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("totalBusinesses");
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static async Task AuthenticateAsync(HttpClient client)
  {
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
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
