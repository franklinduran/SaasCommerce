using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class GetInventoryProductDetailHandler(
  IInventoryReadRepository inventory,
  ICurrentUserService currentUser)
{
  public async Task<Result<InventoryProductDetailResponse>> Handle(
    GetInventoryProductDetailQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<InventoryProductDetailResponse>(InventoryErrors.UserContextRequired);
    }

    if (query.ProductId == Guid.Empty)
    {
      return Result.Failure<InventoryProductDetailResponse>(InventoryErrors.ProductNotFound);
    }

    var detail = await inventory.GetProductDetailAsync(
      new BusinessId(businessId),
      query.ProductId,
      cancellationToken);

    return detail is null
      ? Result.Failure<InventoryProductDetailResponse>(InventoryErrors.ProductNotFound)
      : Result.Success(detail);
  }
}
