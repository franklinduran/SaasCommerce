#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Feedback.Contracts.Requests;
using SaasCommerce.Modules.Feedback.Contracts.Responses;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;

namespace SaasCommerce.Api.Tests;

public sealed class BetaFeedbackEndpointTests
{
  private static int sequence = 3700000;

  [Fact]
  public async Task PostBetaFeedback_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/beta/feedback",
      new CreateBetaFeedbackRequest("Bug", "Venta fallida", "La venta queda procesando.", "/sales/1"));

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PostBetaFeedback_ShouldCreateFeedback_WithAuthenticatedBusiness()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var login = await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/beta/feedback",
      new CreateBetaFeedbackRequest("SaleIssue", "Venta queda procesando", "Despues de cobrar, la venta no completa.", "/sales"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BetaFeedbackResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
    payload.Data!.BusinessId.Should().Be(login.User.BusinessId);
    payload.Data.Status.Should().Be("New");
    payload.Data.Category.Should().Be("SaleIssue");
  }

  [Fact]
  public async Task GetBetaFeedback_ShouldReturnOnlyCurrentBusiness()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    var adminLogin = await AuthenticateAsync(adminClient);
    var adminFeedback = await CreateFeedbackAsync(adminClient, "Problema de inventario", "Stock no actualiza luego de compra.");
    var secondTenant = await RegisterBusinessAsync(factory, "beta-feedback");

    await CreateFeedbackAsync(secondTenant.Client, "Caso de otro negocio", "Este feedback no debe verse.");

    var response = await adminClient.GetAsync("/api/beta/feedback?pageSize=20");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BetaFeedbackListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().Contain(item => item.Id == adminFeedback.Id);
    payload.Data.Items.Should().OnlyContain(item => item.BusinessId == adminLogin.User.BusinessId);
  }

  [Fact]
  public async Task PutBetaFeedbackStatus_ShouldUpdateStatus_WhenAdminManagesFeedback()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var feedback = await CreateFeedbackAsync(client, "Duda de caja", "No queda claro el cierre diario.");

    var response = await client.PutAsJsonAsync(
      $"/api/beta/feedback/{feedback.Id}/status",
      new UpdateBetaFeedbackStatusRequest("InReview", "Lo revisa soporte."));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BetaFeedbackResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Status.Should().Be("InReview");
    payload.Data.ReviewNote.Should().Be("Lo revisa soporte.");
  }

  [Fact]
  public async Task PostBetaFeedback_ShouldReturnValidationError_WhenCategoryIsInvalid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/beta/feedback",
      new CreateBetaFeedbackRequest("Other", "Caso invalido", "Categoria no permitida.", null));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BetaFeedbackResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
  }

  private static async Task<BetaFeedbackResponse> CreateFeedbackAsync(
    HttpClient client,
    string title,
    string description)
  {
    var response = await client.PostAsJsonAsync(
      "/api/beta/feedback",
      new CreateBetaFeedbackRequest("Bug", title, description, "/beta-feedback"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BetaFeedbackResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    return payload.Data!;
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

  private static async Task<RegisteredTenant> RegisterBusinessAsync(
    WebApplicationFactory<Program> factory,
    string prefix)
  {
    var client = factory.CreateClient();
    var response = await client.PostAsJsonAsync(
      "/api/account/register-business",
      new RegisterBusinessRequest(
        $"Colmado {prefix}",
        "Duenio Local",
        $"{prefix}-{Guid.NewGuid():N}@example.com",
        "Admin123!",
        "Rnc",
        NextIdentificationNumber(),
        [new RegisterBusinessPhoneRequest(NextPhoneNumber(), "Principal", true)],
        "Principal",
        Guid.Parse("11111111-1111-1111-1111-111111111111")));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterBusinessResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      payload.Data!.AccessToken);

    return new RegisteredTenant(client);
  }

  private static string NextPhoneNumber()
    => $"829{Interlocked.Increment(ref sequence):D7}";

  private static string NextIdentificationNumber()
    => $"1{Interlocked.Increment(ref sequence):D8}";

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

  private sealed record RegisteredTenant(HttpClient Client);
}

#pragma warning restore CA1707
