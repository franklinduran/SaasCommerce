using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherHostedService(
  IServiceScopeFactory scopeFactory,
  OutboxTrigger trigger,
  IOptions<OutboxPublisherOptions> options,
  ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
  private static readonly Action<ILogger, int, int, Exception?> LogStarted =
    LoggerMessage.Define<int, int>(
      LogLevel.Information,
      new EventId(2210, nameof(LogStarted)),
      "Outbox publisher started. BatchSize={BatchSize} FallbackIntervalMs={FallbackIntervalMs}");
  private static readonly Action<ILogger, Exception?> LogIterationFailed =
    LoggerMessage.Define(
      LogLevel.Error,
      new EventId(2211, nameof(LogIterationFailed)),
      "Outbox publisher iteration failed.");
  private static readonly Action<ILogger, Exception?> LogIterationCanceled =
    LoggerMessage.Define(
      LogLevel.Debug,
      new EventId(2212, nameof(LogIterationCanceled)),
      "Outbox publisher iteration canceled.");

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var opts = options.Value;
    // PollingIntervalMs is now the emergency fallback only — normal wakeup is
    // via OutboxTrigger (in-process) or OutboxNotificationHostedService (pg_notify).
    // Default to 30 s so the fallback almost never fires under normal operation.
    var fallbackMs = opts.PollingIntervalSeconds.HasValue
      ? opts.PollingIntervalSeconds.Value * 1000
      : Math.Max(50, opts.PollingIntervalMs);

    LogStarted(logger, opts.BatchSize, fallbackMs, null);

    // Drain any messages that accumulated before this process started.
    await PublishPendingAsync(stoppingToken).ConfigureAwait(false);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        // Wake immediately when EfUnitOfWork signals a same-process DB write.
        // The fallback timer handles cross-process messages (e.g. API writes to the
        // outbox table but only the Worker runs this publisher).
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(fallbackMs);
        await trigger.Reader.ReadAsync(cts.Token).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }
      catch (OperationCanceledException)
      {
        // Fallback timer expired — poll for cross-process pending messages.
      }

      await PublishPendingAsync(stoppingToken).ConfigureAwait(false);
    }
  }

  private async Task PublishPendingAsync(CancellationToken cancellationToken)
  {
    try
    {
      await using var scope = scopeFactory.CreateAsyncScope();
      var publisher = scope.ServiceProvider.GetRequiredService<OutboxPublisher>();
      await publisher.PublishPendingAsync(cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      LogIterationCanceled(logger, null);
    }
    catch (Exception exception)
    {
      LogIterationFailed(logger, exception);
    }
  }
}
