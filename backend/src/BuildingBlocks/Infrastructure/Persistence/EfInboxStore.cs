using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

public sealed class EfInboxStore(AppDbContext dbContext) : IInboxStore
{
  public Task<bool> HasProcessedAsync(
    Guid eventId,
    CancellationToken cancellationToken = default)
    => dbContext.InboxMessages.AnyAsync(message => message.EventId == eventId, cancellationToken);

  public async Task MarkProcessedAsync(
    Guid eventId,
    Guid businessId,
    string messageType,
    DateTimeOffset processedAt,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(messageType);

    if (await HasProcessedAsync(eventId, cancellationToken).ConfigureAwait(false))
    {
      return;
    }

    dbContext.InboxMessages.Add(new InboxMessage(eventId, businessId, messageType, processedAt));

    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
  }
}
