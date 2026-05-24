using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SaasCommerce.Api.Tests;

/// <summary>
/// Verifies that every HTTP response from the API carries the required
/// security headers added by SecurityHeadersMiddleware (HU-29.6).
/// </summary>
public sealed class SecurityHeadersTests
{
  [Theory]
  [InlineData("/api/auth/login")]
  [InlineData("/api/me")]
  [InlineData("/api/health")]
  public async Task EveryResponseShouldContainSecurityHeaders(string path)
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    // Use GET or POST depending on endpoint; an unauthenticated call is enough
    // to verify middleware fires before authentication.
    var response = path.Contains("/api/auth/login")
      ? await client.PostAsync(path, null)
      : await client.GetAsync(path);

    response.Headers
      .TryGetValues("X-Content-Type-Options", out var xContentType)
      .Should().BeTrue();
    xContentType!.Should().ContainSingle().Which.Should().Be("nosniff");

    response.Headers
      .TryGetValues("X-Frame-Options", out var xFrame)
      .Should().BeTrue();
    xFrame!.Should().ContainSingle().Which.Should().Be("DENY");

    response.Headers
      .TryGetValues("Referrer-Policy", out var referrer)
      .Should().BeTrue();
    referrer!.Should().ContainSingle().Which.Should().Be("strict-origin-when-cross-origin");

    response.Headers
      .TryGetValues("X-XSS-Protection", out var xss)
      .Should().BeTrue();
    xss!.Should().ContainSingle().Which.Should().Be("0");

    response.Headers
      .TryGetValues("Permissions-Policy", out var permissions)
      .Should().BeTrue();
    permissions!.Should().ContainSingle().Which.Should().Be("camera=(), microphone=(), geolocation=()");
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
