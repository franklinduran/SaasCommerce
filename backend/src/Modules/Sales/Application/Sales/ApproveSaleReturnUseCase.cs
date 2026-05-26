using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class ApproveSaleReturnUseCase(
  ISaleReturnRepository returns,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IApproveSaleReturnUseCase
{
  public async Task<Result> ExecuteAsync(
    SaleReturnRequestedEventV1 message,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(message);

    var saleReturn = await returns.GetAsync(
      new BusinessId(message.BusinessId),
      message.SaleReturnId,
      cancellationToken);

    if (saleReturn is null)
    {
      return Result.Failure(SalesErrors.SaleReturnNotFound);
    }

    try
    {
      saleReturn.Approve(clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      await FailAsync(saleReturn, message, "Return cannot be approved.", cancellationToken);
      return Result.Failure(SalesErrors.InvalidSaleReturn);
    }

    await outbox.AddAsync(
      new SaleReturnApprovedEventV1(
        Guid.NewGuid(),
        message.CorrelationId,
        message.BusinessId,
        message.BranchId,
        message.SaleId,
        message.SaleReturnId,
        message.UserId,
        SaleReturnMapper.ToEventItems(saleReturn),
        saleReturn.Total,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  private async Task FailAsync(
    SaleReturn saleReturn,
    SaleReturnRequestedEventV1 message,
    string reason,
    CancellationToken cancellationToken)
  {
    saleReturn.Fail(reason, clock.UtcNow);
    await outbox.AddAsync(
      new SaleReturnFailedEventV1(
        Guid.NewGuid(),
        message.CorrelationId,
        message.BusinessId,
        message.BranchId,
        message.SaleId,
        message.SaleReturnId,
        message.UserId,
        reason,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
