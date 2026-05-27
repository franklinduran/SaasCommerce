using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class CashRegisterClosedConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashRegisterClosedConsumer> logger)
  : IdempotentConsumer<CashRegisterClosedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CashRegisterClosedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "cashRegister.closed",
      context.Message,
      context.CancellationToken);
  }
}
