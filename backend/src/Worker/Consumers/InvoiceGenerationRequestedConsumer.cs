using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class InvoiceGenerationRequestedConsumer(
  ILogger<InvoiceGenerationRequestedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : LoggingIntegrationEventConsumer<InvoiceGenerationRequestedIntegrationEventV1>(logger, inboxStore, clock);
