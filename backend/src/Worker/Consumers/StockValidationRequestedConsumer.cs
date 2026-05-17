using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class StockValidationRequestedConsumer(
  ILogger<StockValidationRequestedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : LoggingIntegrationEventConsumer<StockValidationRequestedIntegrationEventV1>(logger, inboxStore, clock);
