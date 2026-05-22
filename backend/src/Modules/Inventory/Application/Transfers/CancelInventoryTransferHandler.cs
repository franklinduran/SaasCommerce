using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public sealed record CancelInventoryTransferCommand(Guid TransferId);

public sealed class CancelInventoryTransferHandler(
  IInventoryTransferRepository repository,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<InventoryTransferResponse>> Handle(
    CancelInventoryTransferCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<InventoryTransferResponse>> HandleCoreAsync(
    CancelInventoryTransferCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var transfer = await repository.GetByIdAsync(tenantId, command.TransferId, cancellationToken);

    if (transfer is null)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.NotFound);
    }

    if (transfer.Status is InventoryTransferStatus.Completed or InventoryTransferStatus.Cancelled)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.CannotCancel);
    }

    var now = clock.UtcNow;

    try
    {
      transfer.Cancel(now);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.CannotCancel);
    }

    await outbox.AddAsync(
      new InventoryTransferCancelledEventV1(
        Guid.NewGuid(),
        Guid.NewGuid(),
        businessId,
        transfer.Id,
        transfer.SourceBranchId.Value,
        transfer.TargetBranchId.Value,
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(InventoryTransferResponseMapper.ToResponse(transfer));
  }
}
