using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Worker.Sagas.Sales;

/// <summary>
/// Detects sales permanently stuck in Received state (no saga ever created) and
/// resets their SaleCreatedEventV1 outbox messages to Pending so the publisher
/// re-delivers them to the saga consumer.
///
/// This covers edge cases where the saga consumer exhausted its retry budget and
/// the message landed in the error queue without creating a saga state.
/// </summary>
public sealed class StuckSaleWatchdogService(
  IServiceScopeFactory scopeFactory,
  ILogger<StuckSaleWatchdogService> logger) : BackgroundService
{
  private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);
  private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);
  private static readonly TimeSpan OrphanThreshold = TimeSpan.FromMinutes(10);

  private static readonly Action<ILogger, int, Exception?> LogOrphansFound =
    LoggerMessage.Define<int>(
      LogLevel.Warning,
      new EventId(3100, nameof(LogOrphansFound)),
      "StuckSaleWatchdog: {Count} sale(s) stuck in Received with no saga state. Resetting outbox messages for re-delivery.");

  private static readonly Action<ILogger, int, Exception?> LogReset =
    LoggerMessage.Define<int>(
      LogLevel.Warning,
      new EventId(3101, nameof(LogReset)),
      "StuckSaleWatchdog: Reset {Count} SaleCreatedEventV1 outbox message(s) to Pending.");

  private static readonly Action<ILogger, Exception?> LogCycleError =
    LoggerMessage.Define(
      LogLevel.Error,
      new EventId(3102, nameof(LogCycleError)),
      "StuckSaleWatchdog: Watchdog cycle failed.");

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await Task.Delay(StartupDelay, stoppingToken).ConfigureAwait(false);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await RescueOrphanedSalesAsync(stoppingToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }
      catch (Exception ex)
      {
        LogCycleError(logger, ex);
      }

      await Task.Delay(CheckInterval, stoppingToken).ConfigureAwait(false);
    }
  }

  private async Task RescueOrphanedSalesAsync(CancellationToken ct)
  {
    await using var scope = scopeFactory.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // JSONB query is Npgsql-only; skip on InMemory databases.
    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) != true)
    {
      return;
    }

    var threshold = DateTimeOffset.UtcNow - OrphanThreshold;

    // Sales in Received status older than the threshold that have no saga state.
    var orphanedSaleIds = await dbContext.Set<Sale>()
      .Where(s => s.Status == SaleStatus.Received && s.CreatedAt < threshold)
      .Where(s => !dbContext.SaleSagaStates.Any(ss => ss.SaleId == s.Id))
      .Select(s => s.Id)
      .ToListAsync(ct)
      .ConfigureAwait(false);

    if (orphanedSaleIds.Count == 0)
    {
      return;
    }

    LogOrphansFound(logger, orphanedSaleIds.Count, null);

    // Reset their SaleCreatedEventV1 outbox messages from Published → Pending.
    // Uses JSONB to match the saleId field embedded in the event payload
    // (the outbox CorrelationId is a trace ID, not the SaleId).
    // Wrap in IEnumerable<object> to select the overload that accepts CancellationToken.
    // Passing (sql, param, ct) directly resolves to params object[], which treats ct as a SQL parameter.
    object[] sqlParams = [new NpgsqlParameter("saleIds", orphanedSaleIds.ToArray())];
    var affected = await dbContext.Database.ExecuteSqlRawAsync(
      """
      UPDATE public.outbox_messages
      SET "Status" = 'Pending', "PublishedAt" = NULL
      WHERE "Status" = 'Published'
        AND "EventType" LIKE '%SaleCreatedEventV1%'
        AND ("Payload"::jsonb->>'saleId')::uuid = ANY(@saleIds)
      """,
      sqlParams,
      ct)
      .ConfigureAwait(false);

    if (affected > 0)
    {
      LogReset(logger, affected, null);
    }
  }
}
