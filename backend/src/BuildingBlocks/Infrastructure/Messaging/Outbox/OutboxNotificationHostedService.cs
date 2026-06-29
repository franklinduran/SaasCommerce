using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxNotificationHostedService(
  IConfiguration configuration,
  OutboxTrigger trigger,
  ILogger<OutboxNotificationHostedService> logger) : BackgroundService
{
  // Must match the channel used in EfUnitOfWork.PgNotifySql.
  private const string Channel = "saas_outbox";

  private static readonly Action<ILogger, string, Exception?> LogStarted =
    LoggerMessage.Define<string>(
      LogLevel.Information,
      new EventId(2220, nameof(LogStarted)),
      "Outbox LISTEN started on PostgreSQL channel '{Channel}'.");
  private static readonly Action<ILogger, Exception?> LogReconnecting =
    LoggerMessage.Define(
      LogLevel.Warning,
      new EventId(2221, nameof(LogReconnecting)),
      "Outbox LISTEN connection lost. Reconnecting in 3 s.");
  private static readonly Action<ILogger, Exception?> LogDisabled =
    LoggerMessage.Define(
      LogLevel.Information,
      new EventId(2222, nameof(LogDisabled)),
      "No PostgreSQL connection string found — outbox LISTEN/NOTIFY disabled.");

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      LogDisabled(logger, null);
      return;
    }

    LogStarted(logger, Channel, null);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await ListenAsync(connectionString, stoppingToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }
      catch (Exception ex)
      {
        LogReconnecting(logger, ex);
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
      }
    }
  }

  private async Task ListenAsync(string connectionString, CancellationToken stoppingToken)
  {
    await using var conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync(stoppingToken).ConfigureAwait(false);

    conn.Notification += (_, _) => trigger.Signal();

    await using (var cmd = new NpgsqlCommand("LISTEN saas_outbox", conn))
    {
      await cmd.ExecuteNonQueryAsync(stoppingToken).ConfigureAwait(false);
    }

    while (!stoppingToken.IsCancellationRequested)
    {
      await conn.WaitAsync(stoppingToken).ConfigureAwait(false);
    }
  }
}
