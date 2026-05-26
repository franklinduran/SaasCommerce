#pragma warning disable CA1707

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
using SaasCommerce.Modules.Sales.Application.CashRegisters;
using SaasCommerce.Modules.Sales.Contracts.Requests;

namespace SaasCommerce.Api.Tests;

public sealed class CashRegisterEndpointTests
{
  [Fact]
  public async Task OpenCashRegister_ShouldReturn401_WhenUnauthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/cash-registers/open",
      new OpenCashRegisterRequest(Guid.NewGuid(), 500, null));

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task OpenCashRegister_ShouldReturn201_WhenValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var login = await AuthenticateAsync(client);
    var branchId = login.User.BranchId!.Value;

    var response = await client.PostAsJsonAsync(
      "/api/cash-registers/open",
      new OpenCashRegisterRequest(branchId, 1000, "Apertura de prueba"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<OpenCashRegisterResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.CashRegisterId.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetActiveCashRegister_ShouldReturn200_WithNullData_WhenNoOpenRegister()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/cash-registers/active");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CashRegisterDetailResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().BeNull();
  }

  [Fact]
  public async Task CloseCashRegister_ShouldReturn200_AfterOpeningRegister()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var login = await AuthenticateAsync(client);
    var branchId = login.User.BranchId!.Value;

    var openResponse = await client.PostAsJsonAsync(
      "/api/cash-registers/open",
      new OpenCashRegisterRequest(branchId, 500, null));
    var opened = await openResponse.Content.ReadFromJsonAsync<ApiResponse<OpenCashRegisterResponse>>();
    opened!.IsSuccess.Should().BeTrue();

    var closeResponse = await client.PostAsJsonAsync(
      $"/api/cash-registers/{opened.Data!.CashRegisterId}/close",
      new CloseCashRegisterRequest(480, "Cierre de turno"));
    var payload = await closeResponse.Content.ReadFromJsonAsync<ApiResponse<CloseCashRegisterResponse>>();

    closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.CashRegisterId.Should().Be(opened.Data.CashRegisterId);
    payload.Data.OpeningAmount.Should().Be(500);
    payload.Data.CountedAmount.Should().Be(480);
  }

  [Fact]
  public async Task GetDailySummary_ShouldReturn200_WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    var response = await client.GetAsync($"/api/cash-registers/daily-summary?date={today}");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DailyCashRegisterSummaryResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
    payload.Data!.Registers.Should().NotBeNull();
  }

  private static async Task<LoginResponse> AuthenticateAsync(HttpClient client)
  {
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login!.Data!.AccessToken);

    return login.Data;
  }

  private static WebApplicationFactory<Program> CreateFactory()
    => new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
          configuration.AddInMemoryCollection(new Dictionary<string, string?>
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
