using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Worker.Sagas.Sales;

/// <summary>
/// Detects sales permanently stuck in Received or Processing state and resets the
/// relevant outbox messages so the publisher re-delivers them.
///
/// Received with no saga  → resets SaleCreatedEventV1 (saga never started).
/// Processing with saga   → resets the outbox message that matches the saga's
///   current intermediate state (StockValidationPending, InventoryDeductionPending,
///   PaymentRegistrationPending) or SaleCompletedEventV1 when the saga already
///   reached Completed but the consumer never updated the sale.
/// </summary>
public sealed class StuckSaleWatchdogService(
  IServiceScopeFactory scopeFactory,
  ILogger<StuckSaleWatchdogService> logger) : BackgroundService
{
  private static readonly TimeSpan StartupDelay   = TimeSpan.FromMinutes(1);
  private static readonly TimeSpan CheckInterval  = TimeSpan.FromMinutes(5);
  private static readonly TimeSpan OrphanThreshold = TimeSpan.FromMinutes(10);

  // Maps saga CurrentState → LIKE pattern for the outbox EventType column.
  private static readonly Dictionary<string, string> SagaStateToEventTypeLike = new()
  {
    ["StockValidationPending"]     = "%StockValidationRequestedEventV1%",
    ["InventoryDeductionPending"]  = "%InventoryDeductionRequestedEventV1%",
    ["PaymentRegistrationPending"] = "%PaymentRegistrationRequestedEventV1%",
    ["Completed"]                  = "%SaleCompletedEventV1%",
  };

  // ── Log actions ─────────────────────────────────────────────────────────────

  private static readonly Action<ILogger, int, Exception?> LogOrphansFound =
    LoggerMessage.Define<int>(
      LogLevel.Warning,
      new EventId(3100, nameof(LogOrphansFound)),
      "StuckSaleWatchdog [Received]: {Count} sale(s) stuck with no saga state. Resetting SaleCreatedEventV1 outbox messages.");

  private static readonly Action<ILogger, int, Exception?> LogResetReceived =
    LoggerMessage.Define<int>(
      LogLevel.Warning,
      new EventId(3101, nameof(LogResetReceived)),
      "StuckSaleWatchdog [Received]: Reset {Count} SaleCreatedEventV1 outbox message(s) to Pending.");

  private static readonly Action<ILogger, int, Exception?> LogStuckProcessingFound =
    LoggerMessage.Define<int>(
      LogLevel.Warning,
      new EventId(3102, nameof(LogStuckProcessingFound)),
      "StuckSaleWatchdog [Processing]: {Count} sale(s) stuck in Processing. Resetting blocked outbox messages by saga state.");

  private static readonly Action<ILogger, string, int, Exception?> LogResetProcessing =
    LoggerMessage.Define<string, int>(
      LogLevel.Warning,
      new EventId(3103, nameof(LogResetProcessing)),
      "StuckSaleWatchdog [Processing]: Saga state '{SagaState}' — reset {Count} outbox message(s) to Pending.");

  private static readonly Action<ILogger, int, Exception?> LogUnrecoverableProcessing =
    LoggerMessage.Define<int>(
      LogLevel.Error,
      new EventId(3104, nameof(LogUnrecoverableProcessing)),
      "StuckSaleWatchdog [Processing]: {Count} sale(s) in Processing have no saga state and cannot be auto-recovered. Manual intervention required.");

  private static readonly Action<ILogger, Exception?> LogCycleError =
    LoggerMessage.Define(
      LogLevel.Error,
      new EventId(3105, nameof(LogCycleError)),
      "StuckSaleWatchdog: Watchdog cycle failed.");

  // ── Background loop ─────────────────────────────────────────────────────────

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await Task.Delay(StartupDelay, stoppingToken).ConfigureAwait(false);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        await RescueOrphanedReceivedSalesAsync(stoppingToken).ConfigureAwait(false);
        await RescueStuckProcessingSalesAsync(stoppingToken).ConfigureAwait(false);
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

  // ── Received: no saga created ────────────────────────────────────────────────

  private async Task RescueOrphanedReceivedSalesAsync(CancellationToken ct)
  {
    await using var scope = scopeFactory.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) != true)
      return;

    var threshold = DateTimeOffset.UtcNow - OrphanThreshold;

    var orphanedSaleIds = await dbContext.Set<Sale>()
      .Where(s => s.Status == SaleStatus.Received && s.CreatedAt < threshold)
      .Where(s => !dbContext.SaleSagaStates.Any(ss => ss.SaleId == s.Id))
      .Select(s => s.Id)
      .ToListAsync(ct)
      .ConfigureAwait(false);

    if (orphanedSaleIds.Count == 0)
      return;

    LogOrphansFound(logger, orphanedSaleIds.Count, null);

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
      LogResetReceived(logger, affected, null);
  }

  // ── Processing: saga exists but blocked ──────────────────────────────────────

  private async Task RescueStuckProcessingSalesAsync(CancellationToken ct)
  {
    await using var scope = scopeFactory.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) != true)
      return;

    var threshold = DateTimeOffset.UtcNow - OrphanThreshold;

    // Join Processing sales with their saga state to know which outbox message to reset.
    var stuckSales = await (
      from sale in dbContext.Set<Sale>()
      join saga in dbContext.SaleSagaStates on sale.Id equals saga.SaleId
      where sale.Status == SaleStatus.Processing && sale.CreatedAt < threshold
      select new { SaleId = sale.Id, saga.CurrentState }
    ).ToListAsync(ct).ConfigureAwait(false);

    // Separately detect Processing sales with NO saga (unrecoverable — log only).
    var processingSaleIds = await dbContext.Set<Sale>()
      .Where(s => s.Status == SaleStatus.Processing && s.CreatedAt < threshold)
      .Select(s => s.Id)
      .ToListAsync(ct)
      .ConfigureAwait(false);

    var missingSaga = processingSaleIds.Except(stuckSales.Select(x => x.SaleId)).ToList();
    if (missingSaga.Count > 0)
      LogUnrecoverableProcessing(logger, missingSaga.Count, null);

    if (stuckSales.Count == 0)
      return;

    LogStuckProcessingFound(logger, stuckSales.Count, null);

    // Group by saga state and reset the corresponding outbox message type.
    foreach (var group in stuckSales.GroupBy(x => x.CurrentState))
    {
      if (!SagaStateToEventTypeLike.TryGetValue(group.Key, out var eventTypeLike))
        continue;

      var saleIds = group.Select(x => x.SaleId).ToArray();

      // Npgsql requires an explicit NpgsqlParameter for array types;
      // passing the pattern as a string parameter avoids SQL injection.
      object[] sqlParams =
      [
        new NpgsqlParameter("saleIds",       saleIds),
        new NpgsqlParameter("eventTypeLike", eventTypeLike),
      ];

      var affected = await dbContext.Database.ExecuteSqlRawAsync(
        """
        UPDATE public.outbox_messages
        SET "Status" = 'Pending', "PublishedAt" = NULL
        WHERE "Status" = 'Published'
          AND "EventType" LIKE @eventTypeLike
          AND ("Payload"::jsonb->>'saleId')::uuid = ANY(@saleIds)
        """,
        sqlParams,
        ct)
        .ConfigureAwait(false);

      if (affected > 0)
        LogResetProcessing(logger, group.Key, affected, null);
    }
  }
}
