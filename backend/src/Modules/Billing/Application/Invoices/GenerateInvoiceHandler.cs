using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed class GenerateInvoiceHandler(
  IInvoiceRepository invoices,
  IInvoiceSaleReader sales,
  IOutboxWriter outbox,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork) : IGenerateInvoiceUseCase
{
  private const string CompletedSaleStatus = "Completed";

  public Task<Result<InvoiceResponse>> Handle(
    GenerateInvoiceCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<InvoiceResponse>> HandleCoreAsync(
    GenerateInvoiceCommand command,
    CancellationToken cancellationToken)
  {
    if (command.SaleId == Guid.Empty)
    {
      return await FailAsync(command, InvoiceErrors.InvalidInvoice, cancellationToken);
    }

    var businessIdValue = command.BusinessId ?? currentUser.BusinessId;

    if (businessIdValue is not { } businessId || businessId == Guid.Empty)
    {
      return await FailAsync(command, InvoiceErrors.UserContextRequired, cancellationToken);
    }

    var tenantId = new BusinessId(businessId);
    var existing = await invoices.GetBySaleAsync(tenantId, command.SaleId, cancellationToken);

    if (existing is not null)
    {
      return Result.Success(InvoiceResponseMapper.ToResponse(existing));
    }

    var sale = await sales.GetAsync(businessId, command.SaleId, cancellationToken);

    if (sale is null)
    {
      return await FailAsync(command, InvoiceErrors.SaleNotFound, cancellationToken);
    }

    if (!string.Equals(sale.Status, CompletedSaleStatus, StringComparison.Ordinal))
    {
      return await FailAsync(command, InvoiceErrors.InvalidInvoiceState, cancellationToken);
    }

    var now = clock.UtcNow;
    var invoice = Invoice.Issue(
      Guid.NewGuid(),
      sale.SaleId,
      await invoices.GetNextSequenceAsync(tenantId, cancellationToken),
      new InvoiceContext(tenantId, new BranchId(sale.BranchId), sale.CustomerId),
      new InvoiceFinancials(sale.Total, 0, 0, sale.Total),
      now);

    await invoices.AddAsync(invoice, cancellationToken);
    await outbox.AddAsync(
      new InvoiceGeneratedEventV1(
        Guid.NewGuid(),
        command.CorrelationId ?? Guid.NewGuid(),
        invoice.SaleId,
        invoice.BusinessId.Value,
        invoice.BranchId.Value,
        command.UserId ?? currentUser.UserId ?? Guid.Empty,
        command.PaymentId ?? Guid.Empty,
        invoice.Id,
        invoice.Total,
        now),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(InvoiceResponseMapper.ToResponse(invoice));
  }

  private async Task<Result<InvoiceResponse>> FailAsync(
    GenerateInvoiceCommand command,
    DomainError error,
    CancellationToken cancellationToken)
  {
    if (!command.PublishFailureEvent || command.BusinessId is not { } businessId)
    {
      return Result.Failure<InvoiceResponse>(error);
    }

    var now = clock.UtcNow;
    await outbox.AddAsync(
      new InvoiceGenerationFailedEventV1(
        Guid.NewGuid(),
        command.CorrelationId ?? Guid.NewGuid(),
        businessId,
        Guid.Empty,
        command.SaleId,
        Guid.Empty,
        error.Message,
        now),
      cancellationToken);
    await outbox.AddAsync(
      new InvoiceFailedEventV1(
        Guid.NewGuid(),
        command.CorrelationId ?? Guid.NewGuid(),
        command.SaleId,
        businessId,
        Guid.Empty,
        command.UserId ?? Guid.Empty,
        command.PaymentId ?? Guid.Empty,
        error.Message,
        now),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Failure<InvoiceResponse>(error);
  }
}
