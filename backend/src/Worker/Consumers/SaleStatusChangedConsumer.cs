using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleStatusChangedConsumer(
  ILogger<SaleStatusChangedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : LoggingIntegrationEventConsumer<SaleStatusChangedIntegrationEventV1>(logger, inboxStore, clock);
