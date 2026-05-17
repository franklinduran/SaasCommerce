using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class FailSaleUseCase(
  ISaleRepository sales,
  IRealtimeNotifier realtime,
  IClock clock,
  IUnitOfWork unitOfWork) : IFailSaleUseCase
{
  public async Task<Result> ExecuteAsync(
    SaleFailedEventV1 saleFailed,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(saleFailed);

    var sale = await sales.GetAsync(
      new BusinessId(saleFailed.BusinessId),
      saleFailed.SaleId,
      cancellationToken);

    if (sale is null)
    {
      return Result.Failure(SalesErrors.SaleNotFound);
    }

    if (sale.Status is SaleStatus.Failed or SaleStatus.Completed or SaleStatus.Cancelled)
    {
      return Result.Success();
    }

    try
    {
      sale.Fail(saleFailed.Reason, clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(SalesErrors.InvalidSaleState);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);
    await NotifyStatusChangedAsync(saleFailed, cancellationToken);

    return Result.Success();
  }

  private Task NotifyStatusChangedAsync(
    SaleFailedEventV1 saleFailed,
    CancellationToken cancellationToken)
    => realtime.NotifyBusinessAsync(
      saleFailed.BusinessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        saleFailed.CorrelationId,
        saleFailed.SaleId,
        saleFailed.BusinessId,
        saleFailed.BranchId,
        saleFailed.UserId,
        SaleStatus.Failed.ToString(),
        saleFailed.Reason,
        clock.UtcNow),
      cancellationToken);
}
