using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;

namespace SaasCommerce.Worker.Consumers;

/// <summary>
/// Persists an operational notification when an invoice fails to generate.
/// </summary>
public sealed class InvoiceFailedNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  CreateNotificationHandler handler,
  ILogger<InvoiceFailedNotificationConsumer> logger)
  : IdempotentConsumer<InvoiceFailedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InvoiceFailedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var msg = context.Message;

    return handler.Handle(
      new CreateNotificationCommand(
        BusinessId: msg.BusinessId,
        BranchId: msg.BranchId,
        Type: OperationalNotificationType.InvoiceFailed,
        Severity: OperationalNotificationSeverity.Warning,
        Title: "Error al generar recibo",
        Message: $"No se pudo generar el recibo de la venta: {msg.Reason}",
        RelatedEntityId: msg.SaleId,
        RelatedEntityType: "Sale"),
      context.CancellationToken);
  }
}
