using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Worker.Consumers;

public sealed class GenerateInvoiceConsumer(
  ILogger<GenerateInvoiceConsumer> logger,
  IInboxStore inboxStore,
  IClock clock,
  IGenerateInvoiceUseCase useCase)
  : DelegatingIntegrationEventConsumer<InvoiceGenerationRequestedEventV1>(logger, inboxStore, clock)
{
  protected override async Task<Result> ExecuteUseCaseAsync(
    InvoiceGenerationRequestedEventV1 message,
    CancellationToken cancellationToken)
  {
    _ = await useCase.Handle(
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
