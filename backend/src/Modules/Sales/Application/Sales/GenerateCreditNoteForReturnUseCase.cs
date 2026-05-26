using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class GenerateCreditNoteForReturnUseCase(
  ISaleRepository sales,
  ISaleReturnRepository returns,
  ICustomerCreditRepository customerCredits,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IGenerateCreditNoteForReturnUseCase
{
  public async Task<Result> ExecuteAsync(
    SaleReturnApprovedEventV1 message,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(message);

    var businessId = new BusinessId(message.BusinessId);
    if (await returns.GetCreditNoteByReturnAsync(businessId, message.SaleReturnId, cancellationToken) is not null)
    {
      return Result.Success();
    }

    var sale = await sales.GetAsync(businessId, message.SaleId, cancellationToken);
    var saleReturn = await returns.GetAsync(businessId, message.SaleReturnId, cancellationToken);
    if (sale is null || saleReturn is null)
    {
      return Result.Failure(SalesErrors.SaleReturnNotFound);
    }

    CreditNote creditNote;
    try
    {
      creditNote = CreditNote.Generate(
        Guid.NewGuid(),
        sale,
        saleReturn,
        $"NC-{clock.UtcNow:yyyyMMdd}-{message.SaleReturnId.ToString("N")[..8].ToUpperInvariant()}",
        clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(SalesErrors.InvalidSaleReturn);
    }

    await returns.AddCreditNoteAsync(creditNote, cancellationToken);
    await AdjustCustomerCreditAsync(sale, creditNote, message.UserId, cancellationToken);
    await outbox.AddAsync(
      new CreditNoteGeneratedEventV1(
        Guid.NewGuid(),
        message.CorrelationId,
        message.BusinessId,
        message.BranchId,
        message.SaleId,
        message.SaleReturnId,
        creditNote.Id,
        creditNote.CustomerId,
        creditNote.Total,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  private async Task AdjustCustomerCreditAsync(
    Sale sale,
    CreditNote creditNote,
    Guid userId,
    CancellationToken cancellationToken)
  {
    if (!RegisterCreditSaleUseCase.IsCreditSale(sale.PaymentMethod) ||
        sale.CustomerId is not Guid customerId)
    {
      return;
    }

    var account = await customerCredits.GetAccountAsync(sale.BusinessId, customerId, cancellationToken);
    if (account is null || account.CurrentBalance <= 0)
    {
      return;
    }

    var amount = Math.Min(creditNote.Total, account.CurrentBalance);
    if (amount <= 0)
    {
      return;
    }

    var movement = account.ApplyCancellation(
      Guid.NewGuid(),
      sale.Id,
      amount,
      $"Nota de credito {creditNote.Code}",
      userId,
      clock.UtcNow);
    await customerCredits.AddMovementAsync(movement, cancellationToken);
  }
}
