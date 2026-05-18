using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleCompletedConsumer(
  ILogger<SaleCompletedConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  ICompleteSaleUseCase completeSale,
  IGenerateInvoiceUseCase generateInvoice)
  : DelegatingIntegrationEventConsumer<SaleCompletedEventV1>(logger, inboxStore, clock)
{
  protected override async Task<Result> ExecuteUseCaseAsync(
    SaleCompletedEventV1 message,
    CancellationToken cancellationToken)
  {
    var completion = await completeSale.ExecuteAsync(message, cancellationToken);

    if (completion.IsFailure)
    {
      return completion;
    }

    _ = await generateInvoice.Handle(
      new GenerateInvoiceCommand(
        message.SaleId,
        message.CorrelationId,
        message.BusinessId,
        message.UserId,
        message.PaymentId,
        PublishFailureEvent: true),
      cancellationToken);

    return Result.Success();
  }
}
