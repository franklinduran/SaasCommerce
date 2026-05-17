using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class DeductInventoryConsumer(
  ILogger<DeductInventoryConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IDeductSaleInventoryUseCase useCase)
  : DelegatingIntegrationEventConsumer<InventoryDeductionRequestedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    InventoryDeductionRequestedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
