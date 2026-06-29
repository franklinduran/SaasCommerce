using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Signals the outbox publisher after every SaveChanges, including MassTransit saga persistence
/// which bypasses IUnitOfWork and would otherwise miss the wake-up signal.
/// </summary>
public sealed class OutboxTriggerInterceptor(OutboxTrigger trigger) : SaveChangesInterceptor
{
  public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
  {
    trigger.Signal();
    return base.SavedChanges(eventData, result);
  }

  public override ValueTask<int> SavedChangesAsync(
    SaveChangesCompletedEventData eventData,
    int result,
    CancellationToken cancellationToken = default)
  {
    trigger.Signal();
    return base.SavedChangesAsync(eventData, result, cancellationToken);
  }
}
