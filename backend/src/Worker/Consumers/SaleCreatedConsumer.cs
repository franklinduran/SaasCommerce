using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleCreatedConsumer(
  ILogger<SaleCreatedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : LoggingIntegrationEventConsumer<SaleCreatedIntegrationEventV1>(logger, inboxStore, clock);
