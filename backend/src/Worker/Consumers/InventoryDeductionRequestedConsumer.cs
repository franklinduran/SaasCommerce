using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;

namespace SaasCommerce.Worker.Consumers;

public sealed class InventoryDeductionRequestedConsumer(
  ILogger<InventoryDeductionRequestedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock)
  : LoggingIntegrationEventConsumer<InventoryDeductionRequestedIntegrationEventV1>(logger, inboxStore, clock);
