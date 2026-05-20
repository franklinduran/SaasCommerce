using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;

namespace SaasCommerce.Worker.Consumers;

public abstract class AuditingConsumer<TMessage>(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger logger)
  : IdempotentConsumer<TMessage>(inboxStore, clock, logger)
  where TMessage : class, IIntegrationEvent
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<TMessage> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var entry = BuildAuditEntry(context.Message);
    await auditLogWriter.WriteAsync(entry, context.CancellationToken);
  }

  protected abstract AuditEntry BuildAuditEntry(TMessage message);
}
