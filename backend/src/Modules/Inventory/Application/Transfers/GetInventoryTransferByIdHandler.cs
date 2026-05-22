using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public sealed record GetInventoryTransferByIdQuery(Guid TransferId);

public sealed class GetInventoryTransferByIdHandler(
  IInventoryTransferRepository repository,
  ICurrentUserService currentUser)
{
  public Task<Result<InventoryTransferResponse>> Handle(
    GetInventoryTransferByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<InventoryTransferResponse>> HandleCoreAsync(
    GetInventoryTransferByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var transfer = await repository.GetByIdAsync(tenantId, query.TransferId, cancellationToken);

    if (transfer is null)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.NotFound);
    }

    return Result.Success(InventoryTransferResponseMapper.ToResponse(transfer));
  }
}
