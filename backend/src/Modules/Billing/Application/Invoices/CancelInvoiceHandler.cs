using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed class CancelInvoiceHandler(
  IInvoiceRepository invoices,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  ICorrelationIdProvider correlationIdProvider,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<InvoiceResponse>> Handle(
    CancelInvoiceCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (command.InvoiceId == Guid.Empty)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.InvalidInvoice);
    }

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.UserContextRequired);
    }

    var invoice = await invoices.GetAsync(
      new BusinessId(businessId),
      command.InvoiceId,
      cancellationToken);

    if (invoice is null)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.InvoiceNotFound);
    }

    try
    {
      invoice.Cancel(clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.InvalidInvoiceState);
    }

    await outbox.AddAsync(
      new InvoiceCancelledEventV1(
        Guid.NewGuid(),
        GetCorrelationId(),
        invoice.BusinessId.Value,
        invoice.BranchId.Value,
        invoice.SaleId,
        invoice.Id,
        invoice.InvoiceNumber,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(InvoiceResponseMapper.ToResponse(invoice));
  }

  private Guid GetCorrelationId()
    => Guid.TryParse(correlationIdProvider.CorrelationId, out var correlationId)
      ? correlationId
      : Guid.NewGuid();
}
