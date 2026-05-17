using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class InventoryAdjustedConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryAdjustedConsumer> logger)
  : IdempotentConsumer<InventoryAdjustedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<InventoryAdjustedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.Adjusted,
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.StockChanged,
      new InventoryStockChangedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.ProductId,
        null,
        "ManualAdjustment",
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}
