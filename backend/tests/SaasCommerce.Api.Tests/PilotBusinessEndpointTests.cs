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

public sealed class PilotBusinessEndpointTests
{
  [Fact]
  public async Task PostPilotBusinessShouldReturn401WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/admin/pilot-businesses",
      ValidRequest());

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PostPilotBusinessShouldReturn201WhenAdminCreatesNewBusiness()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/admin/pilot-businesses",
      ValidRequest("piloto@colmado.com", "132099999"));

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CreatePilotBusinessResponse>>();
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.BusinessName.Should().Be("Colmado Piloto SRL");
    payload.Data.AdminEmail.Should().Be("piloto@colmado.com");
    payload.Data.TrialEndsAt.Should().NotBeNull();
  }

  [Fact]
  public async Task PostPilotBusinessShouldReturn400WhenEmailAlreadyExists()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    await client.PostAsJsonAsync(
      "/api/admin/pilot-businesses",
      ValidRequest("dup@test.com", "132011111"));

    var response2 = await client.PostAsJsonAsync(
      "/api/admin/pilot-businesses",
      ValidRequest("dup@test.com", "132022222"));

    response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var payload = await response2.Content.ReadFromJsonAsync<ApiResponse<object>>();
    payload!.Error!.Code.Should().Be("PILOT_BUSINESS_DUPLICATE_EMAIL");
  }

  [Fact]
  public async Task PostPilotBusinessShouldReturn400WhenValidationFails()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var badRequest = new CreatePilotBusinessRequest(
      string.Empty, "Rnc", "132001234", "8091234567",
      string.Empty, string.Empty, string.Empty, "short");

    var response = await client.PostAsJsonAsync("/api/admin/pilot-businesses", badRequest);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetOnboardingStatusShouldReturn200WhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/onboarding/status");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<OnboardingStatusResponse>>();
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.BusinessInfoCompleted.Should().BeTrue();
    payload.Data.TotalSteps.Should().Be(4);
  }

  [Fact]
  public async Task GetOnboardingStatusShouldReturn401WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/onboarding/status");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PostCompleteOnboardingStepShouldReturn200WhenStepIsValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsync(
      "/api/onboarding/steps/BusinessInfo/complete",
      null);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task PostCompleteOnboardingStepShouldReturn400WhenStepIsInvalid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsync(
      "/api/onboarding/steps/InvalidStep/complete",
      null);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static CreatePilotBusinessRequest ValidRequest(
    string email = "pilot@test.com",
    string rnc = "132001234") => new(
      "Colmado Piloto SRL",
      "Rnc",
      rnc,
      "8091234567",
      "Sucursal Central",
      "Ana Belkis",
      email,
      "Admin123!");

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
