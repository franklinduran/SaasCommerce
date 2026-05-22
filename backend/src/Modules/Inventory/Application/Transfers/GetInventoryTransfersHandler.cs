using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public sealed record GetInventoryTransfersQuery(
  Guid? SourceBranchId,
  Guid? TargetBranchId,
  string? Status,
  int Page,
  int PageSize);

public sealed class GetInventoryTransfersHandler(
  IInventoryTransferRepository repository,
  ICurrentUserService currentUser)
{
  public Task<Result<InventoryTransferListResponse>> Handle(
    GetInventoryTransfersQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<InventoryTransferListResponse>> HandleCoreAsync(
    GetInventoryTransfersQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<InventoryTransferListResponse>(TransferErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);

    InventoryTransferStatus? status = null;

    if (!string.IsNullOrWhiteSpace(query.Status) &&
        Enum.TryParse<InventoryTransferStatus>(query.Status, true, out var parsedStatus))
    {
      status = parsedStatus;
    }

    var (items, total) = await repository.ListAsync(
      tenantId,
      query.SourceBranchId,
      query.TargetBranchId,
      status,
      query.Page,
      query.PageSize,
      cancellationToken);

    var response = items.Select(InventoryTransferResponseMapper.ToResponse).ToArray();

    return Result.Success(new InventoryTransferListResponse(response, total, query.Page, query.PageSize));
  }
}
