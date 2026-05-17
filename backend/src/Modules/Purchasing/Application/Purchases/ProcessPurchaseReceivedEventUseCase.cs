using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
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
  IUnitOfWork unitOfWork) : IProcessPurchaseReceivedEventUseCase
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

    var result = await receiptProcessor.ProcessAsync(
      purchase,
      purchaseReceived.UserId,
      purchaseReceived.CorrelationId,
      markAsReceived: false,
      cancellationToken);

    if (result.IsFailure)
    {
      return result;
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
