using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class CustomerPaymentRegisteredNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CustomerPaymentRegisteredNotificationConsumer> logger)
  : IdempotentConsumer<CustomerPaymentRegisteredEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CustomerPaymentRegisteredEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      CustomerCreditRealtimeEvents.PaymentRegistered,
      new CustomerPaymentRegisteredNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.CustomerId,
        context.Message.PaymentId,
        context.Message.Amount,
        context.Message.NewBalance,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}
