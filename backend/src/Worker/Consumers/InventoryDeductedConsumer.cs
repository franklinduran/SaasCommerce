using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class InventoryDeductedConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryDeductedConsumer> logger)
  : IdempotentConsumer<InventoryDeductedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InventoryDeductedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.StockChanged,
      new InventoryStockChangedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.BranchId,
        null,
        context.Message.SaleId,
        "SaleDeduction",
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}
