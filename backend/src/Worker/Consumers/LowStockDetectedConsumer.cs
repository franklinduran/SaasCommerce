using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class LowStockDetectedConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<LowStockDetectedConsumer> logger)
  : IdempotentConsumer<LowStockDetectedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<LowStockDetectedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.LowStockDetected,
      new LowStockDetectedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.ProductId,
        context.Message.ProductName,
        context.Message.CurrentStock,
        context.Message.MinimumStock,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}
