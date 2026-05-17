using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisherHostedService(
  IServiceScopeFactory scopeFactory,
  IOptions<OutboxPublisherOptions> options,
  ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
  private static readonly Action<ILogger, int, double, Exception?> LogStarted =
    LoggerMessage.Define<int, double>(
      LogLevel.Information,
      new EventId(2210, nameof(LogStarted)),
      "Outbox publisher started. BatchSize={BatchSize} PollingIntervalSeconds={PollingIntervalSeconds}");
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
    var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollingIntervalSeconds));

    LogStarted(
      logger,
      options.Value.BatchSize,
      pollingInterval.TotalSeconds,
      null);

    using var timer = new PeriodicTimer(pollingInterval);

    while (!stoppingToken.IsCancellationRequested)
    {
      await PublishPendingAsync(stoppingToken).ConfigureAwait(false);

      try
      {
        await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }
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
    catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
    {
      LogIterationCanceled(logger, exception);
    }
    catch (Exception exception)
    {
      LogIterationFailed(logger, exception);
    }
  }
}
