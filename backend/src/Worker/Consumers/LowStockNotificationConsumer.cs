using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;

namespace SaasCommerce.Worker.Consumers;

/// <summary>
/// Persists an operational notification when low stock is detected.
/// The existing LowStockDetectedConsumer sends SignalR; this one persists to DB.
/// </summary>
public sealed class LowStockNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  CreateNotificationHandler handler,
  ILogger<LowStockNotificationConsumer> logger)
  : IdempotentConsumer<LowStockDetectedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<LowStockDetectedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var msg = context.Message;

    var severity = msg.CurrentStock <= 0
      ? OperationalNotificationSeverity.Critical
      : OperationalNotificationSeverity.Warning;

    return handler.Handle(
      new CreateNotificationCommand(
        BusinessId: msg.BusinessId,
        BranchId: msg.BranchId,
        Type: OperationalNotificationType.LowStock,
        Severity: severity,
        Title: "Stock bajo",
        Message: $"'{msg.ProductName}' tiene {msg.CurrentStock} unidades (mínimo: {msg.MinimumStock}).",
        RelatedEntityId: msg.ProductId,
        RelatedEntityType: "Product"),
      context.CancellationToken);
  }
}
