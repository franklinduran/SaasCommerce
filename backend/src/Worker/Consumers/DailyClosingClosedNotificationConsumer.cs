using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

/// <summary>
/// Creates notifications when a daily closing is finalized with negative profit or large cash difference.
/// </summary>
public sealed class DailyClosingClosedNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  CreateNotificationHandler handler,
  ILogger<DailyClosingClosedNotificationConsumer> logger)
  : IdempotentConsumer<DailyClosingClosedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<DailyClosingClosedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var msg = context.Message;

    // Notify on negative profit
    if (msg.EstimatedNetProfit < 0)
    {
      await handler.Handle(
        new CreateNotificationCommand(
          BusinessId: msg.BusinessId,
          BranchId: msg.BranchId,
          Type: OperationalNotificationType.NegativeProfit,
          Severity: OperationalNotificationSeverity.Critical,
          Title: "Ganancia neta negativa",
          Message: $"El cierre del {msg.ClosingDate} reportó una ganancia neta de RD${msg.EstimatedNetProfit:N2}.",
          RelatedEntityId: msg.ClosingId,
          RelatedEntityType: "DailyClosing"),
        context.CancellationToken);
    }

    // Notify on significant cash difference
    if (msg.CashDifference.HasValue && Math.Abs(msg.CashDifference.Value) > 100m)
    {
      var direction = msg.CashDifference.Value > 0 ? "sobrante" : "faltante";
      var amount = Math.Abs(msg.CashDifference.Value);

      await handler.Handle(
        new CreateNotificationCommand(
          BusinessId: msg.BusinessId,
          BranchId: msg.BranchId,
          Type: OperationalNotificationType.CashDifference,
          Severity: OperationalNotificationSeverity.Warning,
          Title: "Diferencia de efectivo en cierre",
          Message: $"El cierre del {msg.ClosingDate} tiene un {direction} de RD${amount:N2}.",
          RelatedEntityId: msg.ClosingId,
          RelatedEntityType: "DailyClosing"),
        context.CancellationToken);
    }
  }
}
