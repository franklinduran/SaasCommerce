namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;

public interface IInboxStore
{
  Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

  Task MarkProcessedAsync(
    Guid eventId,
    Guid businessId,
    string messageType,
    DateTimeOffset processedAt,
    CancellationToken cancellationToken = default);
}
