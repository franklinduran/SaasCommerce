using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleReturnRequestedConsumer(
  ILogger<SaleReturnRequestedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IApproveSaleReturnUseCase useCase)
  : DelegatingIntegrationEventConsumer<SaleReturnRequestedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    SaleReturnRequestedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
