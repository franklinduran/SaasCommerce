using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class RestoreInventoryFromSaleReturnConsumer(
  ILogger<RestoreInventoryFromSaleReturnConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IRestoreInventoryFromSaleReturnUseCase useCase)
  : DelegatingIntegrationEventConsumer<SaleReturnApprovedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    SaleReturnApprovedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
