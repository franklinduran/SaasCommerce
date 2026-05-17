using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class GenerateInvoiceConsumer(
  ILogger<GenerateInvoiceConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IGenerateSaleInvoiceUseCase useCase)
  : DelegatingIntegrationEventConsumer<InvoiceGenerationRequestedEventV1>(logger, inboxStore, clock)
{
  protected override Task<Result> ExecuteUseCaseAsync(
    InvoiceGenerationRequestedEventV1 message,
    CancellationToken cancellationToken)
    => useCase.ExecuteAsync(message, cancellationToken);
}
