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

/// <summary>
/// Verifies the logout endpoint revokes the refresh token so that subsequent
/// refresh calls fail (HU-29.3 — JWT hardening + refresh token revocation).
/// </summary>
public sealed class LogoutEndpointTests
{
  [Fact]
  public async Task LogoutShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/auth/logout",
      new RefreshTokenRequest("any-refresh-token"));

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task LogoutShouldSucceedAndRevokeRefreshToken()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    // 1. Authenticate
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    login!.Data.Should().NotBeNull();

    var accessToken = login.Data!.AccessToken;
    var refreshToken = login.Data.RefreshToken;

    // 2. Logout — revoke the refresh token
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var logoutResponse = await client.PostAsJsonAsync(
      "/api/auth/logout",
      new RefreshTokenRequest(refreshToken));

    logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var logoutPayload = await logoutResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
    logoutPayload!.IsSuccess.Should().BeTrue();

    // 3. Attempt to refresh using the now-revoked token
    client.DefaultRequestHeaders.Authorization = null;
    var refreshResponse = await client.PostAsJsonAsync(
      "/api/auth/refresh",
      new RefreshTokenRequest(refreshToken));

    // The refresh must fail — the token has been revoked
    refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    var refreshPayload = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<object>>();
    refreshPayload!.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public async Task LogoutWithUnknownTokenShouldSucceedSilently()
  {
    // Anti-enumeration: logout with a made-up refresh token must NOT reveal
    // whether the token existed in the system.
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    // Authenticate just to get a valid access token for the request
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login!.Data!.AccessToken);

    var response = await client.PostAsJsonAsync(
      "/api/auth/logout",
      new RefreshTokenRequest("totally-made-up-token-that-does-not-exist"));

    // Must succeed (200 OK) — no hint about whether the token was real
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
    payload!.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task MultipleLogoutsWithSameTokenShouldBeIdempotent()
  {
    // Once revoked, calling logout again with the same token must still return 200.
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    var accessToken = login!.Data!.AccessToken;
    var refreshToken = login.Data.RefreshToken;

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    // First logout
    var first = await client.PostAsJsonAsync(
      "/api/auth/logout",
      new RefreshTokenRequest(refreshToken));
    first.StatusCode.Should().Be(HttpStatusCode.OK);

    // Second logout with the same (now-revoked) token
    var second = await client.PostAsJsonAsync(
      "/api/auth/logout",
      new RefreshTokenRequest(refreshToken));
    second.StatusCode.Should().Be(HttpStatusCode.OK);
    var payload = await second.Content.ReadFromJsonAsync<ApiResponse<object>>();
    payload!.IsSuccess.Should().BeTrue();
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
