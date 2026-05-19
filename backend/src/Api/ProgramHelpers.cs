using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Api;

internal static class ProgramHelpers
{
  internal const int GeneratedJwtSecretBytes = 32;
  private const int RabbitMqDefaultPort = 5672;
  private const int ReadyCheckTimeoutSeconds = 2;

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
