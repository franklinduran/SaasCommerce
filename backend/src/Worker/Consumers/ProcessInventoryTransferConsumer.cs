using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Application.Transfers;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class ProcessInventoryTransferConsumer(
  ILogger<ProcessInventoryTransferConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IProcessInventoryTransferUseCase useCase)
  : DelegatingIntegrationEventConsumer<InventoryTransferRequestedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    InventoryTransferRequestedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
