using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

/// <summary>
/// Notifies branch users in real-time when a daily closing draft is created.
/// </summary>
public sealed class DailyClosingCreatedConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<DailyClosingCreatedConsumer> logger)
  : IdempotentConsumer<DailyClosingCreatedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<DailyClosingCreatedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "daily_closing.created",
      new
      {
        context.Message.ClosingId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.ClosingDate,
        context.Message.TotalSales,
        context.Message.AlertCount,
        context.Message.OccurredAt,
      },
      context.CancellationToken);
  }
}
