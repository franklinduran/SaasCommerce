using System.Diagnostics;
using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public abstract class DelegatingIntegrationEventConsumer<TMessage> : IdempotentConsumer<TMessage>
  where TMessage : class, IIntegrationEvent
{
  private static readonly Action<ILogger, string, string, Guid, Guid, Guid, Exception?> LogStarted =
    LoggerMessage.Define<string, string, Guid, Guid, Guid>(
      LogLevel.Information,
      new EventId(2500, nameof(LogStarted)),
      "Integration event use case started. ConsumerName={ConsumerName} EventName={EventName} EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId}");

  private static readonly Action<ILogger, string, string, Guid, Guid, Guid, long, Exception?> LogCompleted =
    LoggerMessage.Define<string, string, Guid, Guid, Guid, long>(
      LogLevel.Information,
      new EventId(2501, nameof(LogCompleted)),
      "Integration event use case completed. ConsumerName={ConsumerName} EventName={EventName} EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId} ElapsedMilliseconds={ElapsedMilliseconds}");

  private static readonly Action<ILogger, string, string, Guid, Guid, Guid, string, Exception?> LogFailed =
    LoggerMessage.Define<string, string, Guid, Guid, Guid, string>(
      LogLevel.Error,
      new EventId(2502, nameof(LogFailed)),
      "Integration event use case failed. ConsumerName={ConsumerName} EventName={EventName} EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId} ErrorCode={ErrorCode}");
  private readonly ILogger logger;

  protected DelegatingIntegrationEventConsumer(
    ILogger logger,
    IInboxStore inboxStore,
    IClock clock) : base(inboxStore, clock, logger)
  {
    this.logger = logger;
  }

  protected override async Task ConsumeMessageAsync(ConsumeContext<TMessage> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var consumerName = GetType().Name;
    var eventName = typeof(TMessage).Name;
    var elapsed = Stopwatch.StartNew();

    LogStarted(
      logger,
      consumerName,
      eventName,
      context.Message.EventId,
      context.Message.CorrelationId,
      context.Message.BusinessId,
      null);

    try
    {
      var result = await ExecuteUseCaseAsync(context.Message, context.CancellationToken)
        .ConfigureAwait(false);

      if (result.IsFailure)
      {
        throw new InvalidOperationException(result.Error.Code);
      }

      elapsed.Stop();
      LogCompleted(
        logger,
        consumerName,
        eventName,
        context.Message.EventId,
        context.Message.CorrelationId,
        context.Message.BusinessId,
        elapsed.ElapsedMilliseconds,
        null);
    }
    catch (Exception exception)
    {
      elapsed.Stop();
      var errorCode = exception is InvalidOperationException { Message.Length: > 0 }
        ? exception.Message
        : "UNEXPECTED_CONSUMER_ERROR";

      LogFailed(
        logger,
        consumerName,
        eventName,
        context.Message.EventId,
        context.Message.CorrelationId,
        context.Message.BusinessId,
        errorCode,
        exception);
      throw;
    }
  }

  protected abstract Task<Result> ExecuteUseCaseAsync(
    TMessage message,
    CancellationToken cancellationToken);
}
