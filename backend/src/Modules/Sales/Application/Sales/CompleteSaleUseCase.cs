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

public sealed class CompleteSaleUseCase(
  ISaleRepository sales,
  ICashSessionRepository cashSessions,
  IOutboxWriter outbox,
  IRealtimeNotifier realtime,
  IClock clock,
  IUnitOfWork unitOfWork) : ICompleteSaleUseCase
{
  private const string CashPaymentMethod = "Cash";

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

    if (sale.Status is SaleStatus.Failed or SaleStatus.Cancelled)
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

    // Cash sales are registered as CashIn movements in the open session so the
    // drawer balance and movement list stay consistent with system sales.
    if (sale.PaymentMethod == CashPaymentMethod)
    {
      await TryRegisterCashMovementAsync(saleCompleted, sale, cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);
    await NotifyStatusChangedAsync(saleCompleted, cancellationToken);

    return Result.Success();
  }

  private async Task TryRegisterCashMovementAsync(
    SaleCompletedEventV1 saleCompleted,
    Sale sale,
    CancellationToken ct)
  {
    var session = await cashSessions.GetOpenSessionAsync(
      new BusinessId(saleCompleted.BusinessId),
      new BranchId(saleCompleted.BranchId),
      ct);

    // No open session → sale completes normally; no movement recorded.
    if (session is null)
    {
      return;
    }

    var shortCode = sale.Id.ToString("N")[..8].ToUpperInvariant();

    var movement = session.AddMovement(
      Guid.NewGuid(),
      saleCompleted.UserId,
      CashMovementType.CashIn,
      sale.Total,
      $"Venta #{shortCode}",
      clock.UtcNow);

    // Publish the same event RegisterCashMovementHandler uses so that the
    // real-time notification consumer updates any open cash session views.
    await outbox.AddAsync(
      new CashMovementRegisteredEventV1(
        Guid.NewGuid(),
        saleCompleted.CorrelationId,
        movement.Id,
        session.Id,
        saleCompleted.BusinessId,
        saleCompleted.BranchId,
        saleCompleted.UserId,
        movement.Type.ToString(),
        movement.Amount,
        movement.Description,
        movement.CreatedAt),
      ct);
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
