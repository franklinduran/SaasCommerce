using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;

namespace SaasCommerce.Api;

internal static class JwtServiceCollectionExtensions
{
  public static IServiceCollection AddSaasCommerceJwt(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(environment);

    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer();

    services
      .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Configure<IConfiguration, IHostEnvironment>((options, currentConfiguration, currentEnvironment) =>
      {
        ConfigureJwtBearer(options, currentConfiguration, currentEnvironment);
      });

    return services;
  }

  private static void ConfigureJwtBearer(
    JwtBearerOptions options,
    IConfiguration configuration,
    IHostEnvironment environment)
  {
    var secret = configuration["Jwt:Secret"];
    var issuer = configuration["Jwt:Issuer"];
    var audience = configuration["Jwt:Audience"];

    options.RequireHttpsMetadata = !environment.IsDevelopment();
    options.IncludeErrorDetails = environment.IsDevelopment();
    options.Events = new JwtBearerEvents
    {
      OnChallenge = WriteUnauthorizedResponseAsync,
      OnForbidden = context => WriteAuthErrorResponseAsync(
        context.HttpContext,
        StatusCodes.Status403Forbidden,
        "FORBIDDEN",
        "The current user is not allowed to perform this action.")
    };

    if (string.IsNullOrWhiteSpace(secret))
    {
      return;
    }

    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
      ValidIssuer = issuer,
      ValidateAudience = !string.IsNullOrWhiteSpace(audience),
      ValidAudience = audience,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
      ValidateLifetime = true,
      ClockSkew = TimeSpan.FromMinutes(1)
    };
  }

  private static async Task WriteUnauthorizedResponseAsync(JwtBearerChallengeContext context)
  {
    context.HandleResponse();

    await WriteAuthErrorResponseAsync(
      context.HttpContext,
      StatusCodes.Status401Unauthorized,
      "UNAUTHORIZED",
      "Authentication is required.");
  }

  private static async Task WriteAuthErrorResponseAsync(
    HttpContext httpContext,
    int statusCode,
    string code,
    string message)
  {
    var correlationIdProvider = httpContext.RequestServices
      .GetService<ICorrelationIdProvider>();
    var correlationId = correlationIdProvider?.CorrelationId ?? httpContext.TraceIdentifier;

    httpContext.Response.StatusCode = statusCode;
    httpContext.Response.ContentType = "application/json";

    await httpContext.Response.WriteAsJsonAsync(
      ApiResponse.Failure<object?>(
        new ApiError(code, message),
        correlationId));
  }
}
