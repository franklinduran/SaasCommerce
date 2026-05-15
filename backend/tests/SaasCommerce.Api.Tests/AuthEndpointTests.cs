using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SaasCommerce.Api.Tests;

public sealed class AuthEndpointTests
{
  [Fact]
  public async Task MeShouldReturnUnauthorizedWithoutToken()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/me");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error.Should().NotBeNull();
    payload.Error!.Code.Should().Be("unauthorized");
  }

  [Fact]
  public async Task MeShouldReturnCurrentUserWithValidToken()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    login.Should().NotBeNull();
    login!.IsSuccess.Should().BeTrue();
    login.Data.Should().NotBeNull();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login.Data!.AccessToken);

    var response = await client.GetAsync("/api/me");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var me = await response.Content.ReadFromJsonAsync<ApiResponse<AuthUserResponse>>();
    me.Should().NotBeNull();
    me!.IsSuccess.Should().BeTrue();
    me.Data.Should().NotBeNull();
    me.Data!.BusinessId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    me.Data.BranchId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    me.Data.Roles.Should().ContainSingle().Which.Should().Be("Admin");
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
            ["RabbitMq:UseInMemory"] = "true",
            ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
            ["Jwt:Issuer"] = "SaasCommerce.Tests",
            ["Jwt:Audience"] = "SaasCommerce.Tests",
            ["Jwt:AccessTokenMinutes"] = "30"
          });
        });
      });
}
