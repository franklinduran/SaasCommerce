using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class ValidateStockConsumer(
  ILogger<ValidateStockConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IValidateSaleStockUseCase useCase)
  : DelegatingIntegrationEventConsumer<StockValidationRequestedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    StockValidationRequestedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
