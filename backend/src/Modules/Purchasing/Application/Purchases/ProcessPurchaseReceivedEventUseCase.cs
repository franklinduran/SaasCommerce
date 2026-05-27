using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public interface IProcessPurchaseReceivedEventUseCase
{
  Task<Result> ExecuteAsync(
    PurchaseReceivedEventV1 purchaseReceived,
    CancellationToken cancellationToken = default);
}

public sealed class ProcessPurchaseReceivedEventUseCase(
  IPurchaseRepository purchases,
  PurchaseReceiptProcessor receiptProcessor,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock) : IProcessPurchaseReceivedEventUseCase
{
  public async Task<Result> ExecuteAsync(
    PurchaseReceivedEventV1 purchaseReceived,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(purchaseReceived);

    var purchase = await purchases.GetAsync(
      new BusinessId(purchaseReceived.BusinessId),
      purchaseReceived.PurchaseId,
      cancellationToken);

    if (purchase is null)
    {
      return Result.Failure(PurchaseErrors.PurchaseNotFound);
    }

    if (purchase.Status == PurchaseStatus.Completed)
    {
      return Result.Success();
    }

    try
    {
      purchase.StartProcessing(clock.UtcNow);
      var result = await receiptProcessor.ProcessAsync(
        purchase,
        purchaseReceived.UserId,
        purchaseReceived.CorrelationId,
        markAsReceived: false,
        cancellationToken);

      if (result.IsFailure)
      {
        await MarkFailedAsync(purchase, purchaseReceived, result.Error.Code, cancellationToken);
        return Result.Success();
      }

      purchase.MarkInventoryUpdated(clock.UtcNow);
      await outbox.AddAsync(ToInventoryUpdatedEvent(purchase, purchaseReceived.CorrelationId), cancellationToken);
      purchase.Complete(clock.UtcNow);
    }
    catch (InvalidOperationException exception)
    {
      await MarkFailedAsync(purchase, purchaseReceived, exception.Message, cancellationToken);
      return Result.Success();
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  private async Task MarkFailedAsync(
    Purchase purchase,
    PurchaseReceivedEventV1 purchaseReceived,
    string reason,
    CancellationToken cancellationToken)
  {
    purchase.Fail(reason, clock.UtcNow);
    await outbox.AddAsync(
      new PurchaseFailedEventV1(
        Guid.NewGuid(),
        purchaseReceived.CorrelationId,
        purchase.Id,
        purchase.BusinessId.Value,
        purchase.BranchId.Value,
        purchase.SupplierId,
        purchase.UserId,
        reason,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }

  private PurchaseInventoryUpdatedEventV1 ToInventoryUpdatedEvent(Purchase purchase, Guid correlationId)
    => new(
      Guid.NewGuid(),
      correlationId,
      purchase.Id,
      purchase.BusinessId.Value,
      purchase.BranchId.Value,
      purchase.SupplierId,
      purchase.UserId,
      purchase.Total,
      clock.UtcNow);
}
