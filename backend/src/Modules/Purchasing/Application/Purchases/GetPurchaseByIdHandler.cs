using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class GetPurchaseByIdHandler(
  IPurchaseRepository purchases,
  ISupplierRepository suppliers,
  IProductPurchaseReader products,
  IPurchaseMovementReader movements,
  ICurrentUserService currentUser)
{
  public Task<Result<PurchaseResponse>> Handle(
    GetPurchaseByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<PurchaseResponse>> HandleCoreAsync(
    GetPurchaseByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var purchase = await purchases.GetAsync(tenantId, query.PurchaseId, cancellationToken);

    if (purchase is null)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.PurchaseNotFound);
    }

    var supplier = await suppliers.GetAsync(tenantId, purchase.SupplierId, cancellationToken);
    var productMap = await products.ListAsync(
      businessId,
      purchase.Items.Select(item => item.ProductId).Distinct().ToArray(),
      cancellationToken);
    var movementItems = await movements.ListByPurchaseAsync(tenantId, purchase.Id, cancellationToken);

    return Result.Success(PurchaseResponseMapper.ToResponse(
      purchase,
      supplier?.Name,
      productMap,
      movementItems));
  }
}
