using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class CustomerCreditDebitedNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CustomerCreditDebitedNotificationConsumer> logger)
  : IdempotentConsumer<CustomerCreditDebitedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CustomerCreditDebitedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      CustomerCreditRealtimeEvents.CreditDebited,
      new CustomerCreditDebitedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.CustomerId,
        context.Message.SaleId,
        context.Message.Amount,
        context.Message.NewBalance,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}
