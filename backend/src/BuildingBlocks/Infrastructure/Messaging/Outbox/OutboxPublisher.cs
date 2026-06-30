using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class OutboxPublisher(
  AppDbContext dbContext,
  IEventBus eventBus,
  IClock clock,
  IOptions<OutboxPublisherOptions> options,
  ILogger<OutboxPublisher> logger)
{
  private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
  private static readonly Action<ILogger, int, Exception?> LogRescuedStuck =
    LoggerMessage.Define<int>(
      LogLevel.Warning,
      new EventId(2204, nameof(LogRescuedStuck)),
      "Rescued {Count} outbox message(s) stranded in Processing state (likely from a previous crash).");
  private static readonly Action<ILogger, int, Exception?> LogPublishingBatch =
    LoggerMessage.Define<int>(
      LogLevel.Information,
      new EventId(2200, nameof(LogPublishingBatch)),
      "Publishing {OutboxMessageCount} pending outbox messages.");
  private static readonly Action<ILogger, Exception?> LogOutboxTableNotReady =
    LoggerMessage.Define(
      LogLevel.Information,
      new EventId(2203, nameof(LogOutboxTableNotReady)),
      "Outbox table is not ready yet. Publisher will retry on the next iteration.");
  private static readonly Action<ILogger, Guid, Guid, Guid, string, Exception?> LogPublished =
    LoggerMessage.Define<Guid, Guid, Guid, string>(
      LogLevel.Information,
      new EventId(2201, nameof(LogPublished)),
      "Published outbox message. EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId} EventName={EventName}");
  private static readonly Action<ILogger, Guid, Guid, Guid, int, Exception?> LogPublishFailed =
    LoggerMessage.Define<Guid, Guid, Guid, int>(
      LogLevel.Error,
      new EventId(2202, nameof(LogPublishFailed)),
      "Failed to publish outbox message. EventId={EventId} CorrelationId={CorrelationId} BusinessId={BusinessId} Attempts={Attempts}");

  public async Task<int> PublishPendingAsync(CancellationToken cancellationToken = default)
  {
    if (!await IsOutboxTableReadyAsync(cancellationToken).ConfigureAwait(false))
    {
      LogOutboxTableNotReady(logger, null);
      return 0;
    }

    // Reset messages left in Processing state from a previous crash (Npgsql only — not supported on InMemory).
    // Processing + no PublishedAt means StartAttempt() was saved but the publish never completed.
    var isNpgsql = dbContext.Database.ProviderName
      ?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

    if (isNpgsql)
    {
      var rescued = await dbContext.OutboxMessages
        .Where(message => message.Status == OutboxMessageStatus.Processing)
        .ExecuteUpdateAsync(
          setter => setter.SetProperty(m => m.Status, OutboxMessageStatus.Pending),
          cancellationToken)
        .ConfigureAwait(false);

      if (rescued > 0)
      {
        LogRescuedStuck(logger, rescued, null);
      }
    }

    var batchSize = Math.Max(1, options.Value.BatchSize);
    var messages = await dbContext.OutboxMessages
      .Where(message =>
        message.Status == OutboxMessageStatus.Pending ||
        message.Status == OutboxMessageStatus.Failed)
      .OrderBy(message => message.OccurredAt)
      .Take(batchSize)
      .ToListAsync(cancellationToken)
      .ConfigureAwait(false);

    if (messages.Count == 0)
    {
      return 0;
    }

    LogPublishingBatch(logger, messages.Count, null);

    var published = 0;

    foreach (var message in messages)
    {
      await PublishAsync(message, cancellationToken).ConfigureAwait(false);

      if (message.Status == OutboxMessageStatus.Published)
      {
        published++;
      }
    }

    return published;
  }

  private async Task PublishAsync(
    OutboxMessage message,
    CancellationToken cancellationToken)
  {
    message.StartAttempt();
    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    try
    {
      var messageType = ResolveMessageType(message.EventType);
      var integrationEvent = JsonSerializer.Deserialize(message.Payload, messageType, SerializerOptions)
        ?? throw new InvalidOperationException($"Could not deserialize outbox message {message.Id}.");

      await eventBus.PublishAsync(integrationEvent, messageType, cancellationToken).ConfigureAwait(false);

      message.MarkPublished(clock.UtcNow);
      LogPublished(
        logger,
        message.EventId,
        message.CorrelationId,
        message.BusinessId,
        messageType.Name,
        null);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
      message.MarkFailed(exception.Message);
      LogPublishFailed(
        logger,
        message.EventId,
        message.CorrelationId,
        message.BusinessId,
        message.Attempts,
        exception);
    }

    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }

  private static Type ResolveMessageType(string eventType)
  {
    var type = Type.GetType(eventType, throwOnError: false);

    if (type is not null)
    {
      return type;
    }

    return AppDomain.CurrentDomain
      .GetAssemblies()
      .Select(assembly => assembly.GetType(eventType, throwOnError: false))
      .FirstOrDefault(candidate => candidate is not null)
      ?? throw new InvalidOperationException($"Unknown outbox event type '{eventType}'.");
  }

  private async Task<bool> IsOutboxTableReadyAsync(CancellationToken cancellationToken)
  {
    var providerName = dbContext.Database.ProviderName;

    if (providerName is null ||
        !providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    var connection = dbContext.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;

    if (shouldClose)
    {
      await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
    }

    try
    {
      await using var command = connection.CreateCommand();
      command.CommandText = "SELECT to_regclass('public.outbox_messages') IS NOT NULL";
      var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

      return result is true;
    }
    finally
    {
      if (shouldClose)
      {
        await connection.CloseAsync().ConfigureAwait(false);
      }
    }
  }
}
