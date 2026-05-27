using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Api;

internal static class ProgramHelpers
{
  internal const int GeneratedJwtSecretBytes = 32;
  private const int RabbitMqDefaultPort = 5672;
  private const int ReadyCheckTimeoutSeconds = 2;

  /// <summary>Named rate-limit policy identifiers used across the API.</summary>
  internal static class RateLimitPolicies
  {
    internal const string AuthLogin = "auth-login";
    internal const string AuthRefresh = "auth-refresh";
    internal const string AuthRegister = "auth-register";
  }

  internal static async Task<bool> CanConnectToDatabaseAsync(
    AppDbContext dbContext,
    CancellationToken cancellationToken)
  {
    try
    {
      if (dbContext.Database.ProviderName is null)
      {
        return true;
      }

      return await dbContext.Database.CanConnectAsync(cancellationToken);
    }
    catch (InvalidOperationException)
    {
      return true;
    }
  }

  internal const string StatusHealthy = "Healthy";
  internal const string StatusDegraded = "Degraded";
  internal const string StatusUnhealthy = "Unhealthy";

  internal static async Task<(bool IsHealthy, int StaleCount)> CheckOutboxHealthAsync(
    AppDbContext dbContext,
    CancellationToken cancellationToken)
  {
    try
    {
      var threshold = DateTimeOffset.UtcNow.AddMinutes(-5);
      var staleCount = await dbContext.OutboxMessages
        .CountAsync(
          m => (m.Status == OutboxMessageStatus.Pending || m.Status == OutboxMessageStatus.Failed)
               && m.CreatedAt < threshold,
          cancellationToken);

      return (staleCount == 0, staleCount);
    }
    catch (InvalidOperationException)
    {
      return (true, 0);
    }
  }

  internal static HealthReadyResponse BuildHealthReadyResponse(
    bool databaseReady,
    bool rabbitMqReady,
    bool outboxHealthy,
    int outboxStaleCount)
  {
    string outboxStatus;
    if (outboxHealthy)
    {
      outboxStatus = StatusHealthy;
    }
    else
    {
      outboxStatus = $"{StatusDegraded} ({outboxStaleCount} stale)";
    }

    string overallStatus;
    if (!databaseReady || !rabbitMqReady)
    {
      overallStatus = StatusUnhealthy;
    }
    else if (!outboxHealthy)
    {
      overallStatus = StatusDegraded;
    }
    else
    {
      overallStatus = StatusHealthy;
    }

    return new HealthReadyResponse(
      overallStatus,
      new HealthDependencyStatus("PostgreSQL", databaseReady ? StatusHealthy : StatusUnhealthy),
      new HealthDependencyStatus("RabbitMQ", rabbitMqReady ? StatusHealthy : StatusUnhealthy),
      new HealthDependencyStatus("Outbox", outboxStatus));
  }

  internal static async Task<bool> CanConnectToRabbitMqAsync(
    IConfiguration configuration,
    CancellationToken cancellationToken)
  {
    if (configuration.GetValue<bool>("RabbitMq:UseInMemory"))
    {
      return true;
    }

    var host = configuration["RabbitMq:Host"] ?? "localhost";
    var port = configuration.GetValue<int?>("RabbitMq:Port") ?? RabbitMqDefaultPort;

    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    timeout.CancelAfter(TimeSpan.FromSeconds(ReadyCheckTimeoutSeconds));

    try
    {
      using var client = new TcpClient();
      await client.ConnectAsync(host, port, timeout.Token);

      return client.Connected;
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      return false;
    }
    catch (SocketException)
    {
      return false;
    }
  }

  internal static void ConfigureDevelopmentJwtSecret(
    ConfigurationManager configuration,
    IHostEnvironment environment)
  {
    if (!environment.IsDevelopment() ||
        !string.IsNullOrWhiteSpace(configuration["Jwt:Secret"]))
    {
      return;
    }

    configuration["Jwt:Secret"] = Convert.ToBase64String(
      RandomNumberGenerator.GetBytes(GeneratedJwtSecretBytes));
  }

  internal static async Task WriteStatusCodeResponseAsync(StatusCodeContext statusCodeContext)
  {
    var httpContext = statusCodeContext.HttpContext;

    if (httpContext.Response.HasStarted)
    {
      return;
    }

    var error = httpContext.Response.StatusCode switch
    {
      StatusCodes.Status401Unauthorized => new ApiError(
        ApiErrorCodes.Unauthorized,
        "Authentication is required."),
      StatusCodes.Status403Forbidden => new ApiError(
        ApiErrorCodes.Forbidden,
        "The current user is not allowed to perform this action."),
      StatusCodes.Status404NotFound => new ApiError(
        ApiErrorCodes.NotFound,
        "The requested resource was not found."),
      StatusCodes.Status429TooManyRequests => new ApiError(
        ApiErrorCodes.TooManyRequests,
        "Too many requests. Please try again later."),
      _ => null
    };

    if (error is null)
    {
      return;
    }

    httpContext.Response.ContentType = "application/json";

    var correlationId = httpContext.RequestServices
      .GetService<ICorrelationIdProvider>()?
      .CorrelationId ?? httpContext.TraceIdentifier;

    await httpContext.Response.WriteAsJsonAsync(
      ApiResponse.Failure<object?>(error, correlationId));
  }

  internal static async Task MigrateDatabaseAsync(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(serviceProvider);

    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.IsRelational())
    {
      await dbContext.Database.MigrateAsync(cancellationToken);
    }
  }

  internal static string[] GetCorsAllowedOrigins(IConfiguration configuration)
  {
    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

    return origins is { Length: > 0 }
      ? origins
      : ["http://localhost:5173", "http://127.0.0.1:5173"];
  }

  internal static IEnumerable<string> GetAllSystemPermissions()
    => typeof(SystemPermissions)
      .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
      .Where(f => f.IsLiteral && f.FieldType == typeof(string))
      .Select(f => (string)f.GetValue(null)!);
}
