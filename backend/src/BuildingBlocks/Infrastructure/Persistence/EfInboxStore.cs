using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

public sealed class EfInboxStore(AppDbContext dbContext) : IInboxStore
{
  public Task<bool> HasProcessedAsync(
    Guid eventId,
    string consumerName,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);

    return dbContext.InboxMessages.AnyAsync(
      message => message.EventId == eventId && message.ConsumerName == consumerName,
      cancellationToken);
  }

  public async Task MarkProcessedAsync(
    Guid eventId,
    string consumerName,
    Guid businessId,
    Guid correlationId,
    DateTimeOffset processedAt,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);

    if (await HasProcessedAsync(eventId, consumerName, cancellationToken).ConfigureAwait(false))
    {
      return;
    }

    dbContext.InboxMessages.Add(
      new InboxMessage(
        eventId,
        consumerName,
        businessId,
        correlationId,
        processedAt,
        processedAt));

    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }
}
