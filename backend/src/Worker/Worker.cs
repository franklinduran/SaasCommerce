namespace SaasCommerce.Worker;

public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
  private static readonly Action<ILogger, Exception?> LogWorkerStarted =
    LoggerMessage.Define(
      LogLevel.Information,
      new EventId(1000, nameof(LogWorkerStarted)),
      "SaasCommerce worker started.");

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    LogWorkerStarted(logger, null);

    await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
  }
}
