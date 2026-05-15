using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class GetStockHandler(
  IInventoryRepository inventory,
  ICurrentUserService currentUser)
{
  public async Task<Result<StockListResponse>> Handle(
    GetStockQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<StockListResponse>(InventoryErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);
    var total = await inventory.CountStockAsync(tenantId, cancellationToken);
    var items = await inventory.ListStockAsync(tenantId, page, pageSize, cancellationToken);

    return Result.Success(new StockListResponse(
      items.Select(InventoryResponseMapper.ToResponse).ToArray(),
      page,
      pageSize,
      total));
  }
}
