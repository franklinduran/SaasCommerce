using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class CompleteSaleUseCase(
  ISaleRepository sales,
  IRealtimeNotifier realtime,
  IClock clock,
  IUnitOfWork unitOfWork) : ICompleteSaleUseCase
{
  public async Task<Result> ExecuteAsync(
    SaleCompletedEventV1 saleCompleted,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(saleCompleted);

    var sale = await sales.GetAsync(
      new BusinessId(saleCompleted.BusinessId),
      saleCompleted.SaleId,
      cancellationToken);

    if (sale is null)
    {
      return Result.Failure(SalesErrors.SaleNotFound);
    }

    if (sale.Status == SaleStatus.Completed)
    {
      return Result.Success();
    }

    try
    {
      sale.Complete(clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(SalesErrors.InvalidSaleState);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);
    await NotifyStatusChangedAsync(saleCompleted, cancellationToken);

    return Result.Success();
  }

  private Task NotifyStatusChangedAsync(
    SaleCompletedEventV1 saleCompleted,
    CancellationToken cancellationToken)
    => realtime.NotifyBusinessAsync(
      saleCompleted.BusinessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        saleCompleted.CorrelationId,
        saleCompleted.SaleId,
        saleCompleted.BusinessId,
        saleCompleted.BranchId,
        saleCompleted.UserId,
        SaleStatus.Completed.ToString(),
        null,
        clock.UtcNow),
      cancellationToken);
}
