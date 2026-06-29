using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Wakes the outbox publisher after a database transaction commits.
/// This is the correct hook for MassTransit saga persistence, which wraps every state
/// transition in an explicit PostgreSQL transaction — SaveChangesAsync flushes rows but
/// they remain invisible to other connections until CommitAsync is called. Relying only on
/// the SaveChangesInterceptor would fire the trigger before the commit, causing the publisher
/// to see zero pending rows and fall back to the 30-second polling timer instead of
/// publishing immediately.
/// </summary>
public sealed class OutboxTransactionInterceptor(OutboxTrigger trigger) : DbTransactionInterceptor
{
  public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    => trigger.Signal();

  public override Task TransactionCommittedAsync(
    DbTransaction transaction,
    TransactionEndEventData eventData,
    CancellationToken cancellationToken = default)
  {
    trigger.Signal();
    return Task.CompletedTask;
  }
}
