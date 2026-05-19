using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class ReceivePurchaseHandler( // NOSONAR S107 — DI constructor injection
  IPurchaseRepository purchases,
  ISupplierRepository suppliers,
  IProductPurchaseReader products,
  PurchaseReceiptProcessor receiptProcessor,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  ICorrelationIdProvider correlationIdProvider,
  IUnitOfWork unitOfWork)
{
  public Task<Result<PurchaseResponse>> Handle(
    ReceivePurchaseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<PurchaseResponse>> HandleCoreAsync(
    ReceivePurchaseCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var purchase = await purchases.GetAsync(tenantId, command.PurchaseId, cancellationToken);

    if (purchase is null)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.PurchaseNotFound);
    }

    var correlationId = ResolveCorrelationId();
    var receipt = await receiptProcessor.ProcessAsync(
      purchase,
      userId,
      correlationId,
      markAsReceived: true,
      cancellationToken);

    if (receipt.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(receipt.Error);
    }

    await outbox.AddAsync(
      CreatePurchaseHandler.ToReceivedEvent(purchase, correlationId),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    var supplier = await suppliers.GetAsync(tenantId, purchase.SupplierId, cancellationToken);
    var productMap = await products.ListAsync(
      businessId,
      purchase.Items.Select(item => item.ProductId).Distinct().ToArray(),
      cancellationToken);

    return Result.Success(PurchaseResponseMapper.ToResponse(purchase, supplier?.Name, productMap));
  }

  private Guid ResolveCorrelationId()
    => Guid.TryParse(correlationIdProvider.CorrelationId, out var parsedValue)
      ? parsedValue
      : Guid.NewGuid();
}
