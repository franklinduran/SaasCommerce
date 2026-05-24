using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

/// <summary>
/// Persists an operational notification when a sale fails.
/// </summary>
public sealed class SaleFailedNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  CreateNotificationHandler handler,
  ILogger<SaleFailedNotificationConsumer> logger)
  : IdempotentConsumer<SaleFailedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<SaleFailedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var msg = context.Message;

    return handler.Handle(
      new CreateNotificationCommand(
        BusinessId: msg.BusinessId,
        BranchId: msg.BranchId,
        Type: OperationalNotificationType.SaleFailed,
        Severity: OperationalNotificationSeverity.Critical,
        Title: "Venta fallida",
        Message: $"La venta no pudo procesarse: {msg.Reason}",
        RelatedEntityId: msg.SaleId,
        RelatedEntityType: "Sale"),
      context.CancellationToken);
  }
}
