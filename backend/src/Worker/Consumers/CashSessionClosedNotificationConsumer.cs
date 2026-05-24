using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

/// <summary>
/// Creates a CashDifference notification when a cash session closes with a significant discrepancy.
/// Only fires when the difference exceeds 50 (absolute value).
/// </summary>
public sealed class CashSessionClosedNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  CreateNotificationHandler handler,
  ILogger<CashSessionClosedNotificationConsumer> logger)
  : IdempotentConsumer<CashSessionClosedEventV1>(inboxStore, clock, logger)
{
  private const decimal SignificantDifferenceThreshold = 50m;

  protected override Task ConsumeMessageAsync(ConsumeContext<CashSessionClosedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var msg = context.Message;
    var difference = msg.ClosingBalance - msg.SystemBalance;

    // Only create a notification when difference is significant
    if (Math.Abs(difference) <= SignificantDifferenceThreshold)
      return Task.CompletedTask;

    var severity = Math.Abs(difference) > 500m
      ? OperationalNotificationSeverity.Critical
      : OperationalNotificationSeverity.Warning;

    var direction = difference > 0 ? "sobrante" : "faltante";
    var amount = Math.Abs(difference);

    return handler.Handle(
      new CreateNotificationCommand(
        BusinessId: msg.BusinessId,
        BranchId: msg.BranchId,
        Type: OperationalNotificationType.CashDifference,
        Severity: severity,
        Title: "Diferencia en caja",
        Message: $"Se detectó un {direction} de RD${amount:N2} al cerrar la caja.",
        RelatedEntityId: msg.CashSessionId,
        RelatedEntityType: "CashSession"),
      context.CancellationToken);
  }
}
