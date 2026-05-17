using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class CancelPurchaseHandler(
  IPurchaseRepository purchases,
  ISupplierRepository suppliers,
  IProductPurchaseReader products,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  ICorrelationIdProvider correlationIdProvider,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<PurchaseResponse>> Handle(
    CancelPurchaseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<PurchaseResponse>> HandleCoreAsync(
    CancelPurchaseCommand command,
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

    try
    {
      purchase.Cancel(clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.InvalidPurchaseState);
    }

    await outbox.AddAsync(
      new PurchaseCancelledEventV1(
        Guid.NewGuid(),
        ResolveCorrelationId(),
        purchase.Id,
        purchase.BusinessId.Value,
        purchase.BranchId.Value,
        purchase.SupplierId,
        userId,
        clock.UtcNow),
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
