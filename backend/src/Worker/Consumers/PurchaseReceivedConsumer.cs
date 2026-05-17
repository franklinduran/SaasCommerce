using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class PurchaseReceivedConsumer(
  ILogger<PurchaseReceivedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IProcessPurchaseReceivedEventUseCase useCase)
  : DelegatingIntegrationEventConsumer<PurchaseReceivedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    PurchaseReceivedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
