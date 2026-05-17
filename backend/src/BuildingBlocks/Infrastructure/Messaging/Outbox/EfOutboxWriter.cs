using System.Text.Json;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

public sealed class EfOutboxWriter(
  AppDbContext dbContext,
  IClock clock) : IOutboxWriter
{
  private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

  public Task AddAsync<TEvent>(
    TEvent integrationEvent,
    CancellationToken cancellationToken = default)
    where TEvent : class, IIntegrationEvent
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var eventType = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? typeof(TEvent).Name;
    var payload = JsonSerializer.Serialize(integrationEvent, SerializerOptions);

    dbContext.OutboxMessages.Add(
      new OutboxMessage(
        integrationEvent.EventId,
        integrationEvent.CorrelationId,
        integrationEvent.BusinessId,
        eventType,
        payload,
        integrationEvent.OccurredAt,
        clock.UtcNow));

    return Task.CompletedTask;
  }
}
