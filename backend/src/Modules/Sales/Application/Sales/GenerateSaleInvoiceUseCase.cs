using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class GenerateSaleInvoiceUseCase(
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IGenerateSaleInvoiceUseCase
{
  public async Task<Result> ExecuteAsync(
    InvoiceGenerationRequestedEventV1 invoiceGenerationRequested,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(invoiceGenerationRequested);

    if (invoiceGenerationRequested.PaymentId == Guid.Empty)
    {
      await outbox.AddAsync(
        new InvoiceFailedEventV1(
          Guid.NewGuid(),
          invoiceGenerationRequested.CorrelationId,
          invoiceGenerationRequested.SaleId,
          invoiceGenerationRequested.BusinessId,
          invoiceGenerationRequested.BranchId,
          invoiceGenerationRequested.UserId,
          invoiceGenerationRequested.PaymentId,
          "Payment id is required before invoice generation.",
          clock.UtcNow),
        cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
      return Result.Success();
    }

    await outbox.AddAsync(
      new InvoiceGeneratedEventV1(
        Guid.NewGuid(),
        invoiceGenerationRequested.CorrelationId,
        invoiceGenerationRequested.SaleId,
        invoiceGenerationRequested.BusinessId,
        invoiceGenerationRequested.BranchId,
        invoiceGenerationRequested.UserId,
        invoiceGenerationRequested.PaymentId,
        Guid.NewGuid(),
        invoiceGenerationRequested.Total,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
