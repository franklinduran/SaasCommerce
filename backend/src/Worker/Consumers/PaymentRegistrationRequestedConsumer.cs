using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class PaymentRegistrationRequestedConsumer(
  ILogger<PaymentRegistrationRequestedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : LoggingIntegrationEventConsumer<PaymentRegistrationRequestedIntegrationEventV1>(logger, inboxStore, clock);
