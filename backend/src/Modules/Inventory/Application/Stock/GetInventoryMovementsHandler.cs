using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class GetInventoryMovementsHandler(
  IInventoryRepository inventory,
  ICurrentUserService currentUser)
{
  public Task<Result<InventoryMovementListResponse>> Handle(
    GetInventoryMovementsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<InventoryMovementListResponse>> HandleCoreAsync(
    GetInventoryMovementsQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<InventoryMovementListResponse>(InventoryErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);
    var total = await inventory.CountMovementsAsync(tenantId, query.ProductId, cancellationToken);
    var items = await inventory.ListMovementsAsync(
      tenantId,
      query.ProductId,
      page,
      pageSize,
      cancellationToken);

    return Result.Success(new InventoryMovementListResponse(
      items.Select(InventoryResponseMapper.ToResponse).ToArray(),
      page,
      pageSize,
      total));
  }
}
