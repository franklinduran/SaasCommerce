namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IInboxStore
{
  Task<bool> HasProcessedAsync(
    Guid eventId,
    string consumerName,
    CancellationToken cancellationToken = default);

  Task MarkProcessedAsync(
    Guid eventId,
    string consumerName,
    Guid businessId,
    Guid correlationId,
    DateTimeOffset processedAt,
    CancellationToken cancellationToken = default);
}
