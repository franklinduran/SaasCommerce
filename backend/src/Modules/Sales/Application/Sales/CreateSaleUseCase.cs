using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class CreateSaleUseCase(
  ISaleRepository sales,
  IOutboxWriter outbox,
  IRealtimeNotifier realtime,
  IClock clock,
  IUnitOfWork unitOfWork) : ICreateSaleUseCase
{
  public async Task<Result> ExecuteAsync(
    SaleCreatedEventV1 saleCreated,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(saleCreated);

    var existingSale = await sales.GetAsync(
      new BusinessId(saleCreated.BusinessId),
      saleCreated.SaleId,
      cancellationToken);

    if (existingSale is not null)
    {
      return Result.Success();
    }

    Sale sale;

    try
    {
      sale = Sale.Create(
        saleCreated.SaleId,
        new BusinessId(saleCreated.BusinessId),
        new BranchId(saleCreated.BranchId),
        saleCreated.UserId,
        saleCreated.Items.Select(item => new SaleLine(item.ProductId, item.Quantity, item.UnitPrice)).ToArray(),
        saleCreated.PaymentMethod,
        saleCreated.CreatedAt);

      sale.MarkAsProcessing(clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure(SalesErrors.InvalidSale);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(SalesErrors.InvalidSale);
    }

    await sales.AddAsync(sale, cancellationToken);
    await outbox.AddAsync(saleCreated, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    await NotifyStatusChangedAsync(
      saleCreated,
      SaleStatus.Processing.ToString(),
      null,
      cancellationToken);

    return Result.Success();
  }

  private Task NotifyStatusChangedAsync(
    SaleCreatedEventV1 saleCreated,
    string status,
    string? reason,
    CancellationToken cancellationToken)
    => realtime.NotifyBusinessAsync(
      saleCreated.BusinessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        saleCreated.CorrelationId,
        saleCreated.SaleId,
        saleCreated.BusinessId,
        saleCreated.BranchId,
        saleCreated.UserId,
        status,
        reason,
        clock.UtcNow),
      cancellationToken);
}
