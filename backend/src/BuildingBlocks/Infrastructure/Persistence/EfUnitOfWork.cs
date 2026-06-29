using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

public sealed class EfUnitOfWork(
  AppDbContext dbContext,
  OutboxTrigger outboxTrigger,
  ILogger<EfUnitOfWork> logger) : IUnitOfWork
{
  // Must match the channel in OutboxNotificationHostedService.
  private const string PgNotifySql = "SELECT pg_notify('saas_outbox', '')";

  private static readonly Action<ILogger, Exception?> LogPgNotifyFailed =
    LoggerMessage.Define(
      LogLevel.Warning,
      new EventId(2230, nameof(LogPgNotifyFailed)),
      "pg_notify to outbox channel failed; the fallback timer will cover pending messages.");

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    var result = await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    // In-process wake-up for same-process workers (sub-1 ms).
    outboxTrigger.Signal();

    // Cross-process wake-up: notify other processes (e.g. Worker) listening on the
    // PostgreSQL channel so they publish outbox messages without waiting for the fallback timer.
    await TryPgNotifyAsync(cancellationToken).ConfigureAwait(false);

    return result;
  }

  private async Task TryPgNotifyAsync(CancellationToken cancellationToken)
  {
    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) != true)
      return;

    try
    {
      await dbContext.Database
        .ExecuteSqlRawAsync(PgNotifySql, cancellationToken)
        .ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      // Shutdown in progress — the outbox will drain on next startup.
    }
    catch (Exception ex)
    {
      LogPgNotifyFailed(logger, ex);
    }
  }
}
