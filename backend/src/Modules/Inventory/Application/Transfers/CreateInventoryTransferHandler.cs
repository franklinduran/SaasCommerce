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

public sealed class CreateInventoryTransferHandler(
  IInventoryTransferRepository repository,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<InventoryTransferResponse>> Handle(
    CreateInventoryTransferCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<InventoryTransferResponse>> HandleCoreAsync(
    CreateInventoryTransferCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.UserContextRequired);
    }

    if (command.SourceBranchId == command.TargetBranchId)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.SameBranch);
    }

    if (command.Items is null || command.Items.Count == 0)
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.NoItems);
    }

    if (command.Items.Any(item => item.Quantity <= 0))
    {
      return Result.Failure<InventoryTransferResponse>(TransferErrors.InvalidQuantity);
    }

    var tenantId = new BusinessId(businessId);
    var sourceBranchId = new BranchId(command.SourceBranchId);
    var targetBranchId = new BranchId(command.TargetBranchId);
    var transferId = Guid.NewGuid();
    var now = clock.UtcNow;

    var items = command.Items
      .Select(item => new InventoryTransferItem(transferId, item.ProductId, item.Quantity))
      .ToArray();

    InventoryTransfer transfer;

    try
    {
      transfer = new InventoryTransfer(
        transferId,
        tenantId,
        sourceBranchId,
        targetBranchId,
        userId,
        items,
        now,
        command.Note);
    }
    catch (InvalidOperationException ex)
    {
      return Result.Failure<InventoryTransferResponse>(new DomainError("TRANSFER_INVALID", ex.Message));
    }

    await repository.AddAsync(transfer, cancellationToken);

    var correlationId = Guid.NewGuid();

    await outbox.AddAsync(
      new InventoryTransferRequestedEventV1(
        Guid.NewGuid(),
        correlationId,
        businessId,
        transferId,
        command.SourceBranchId,
        command.TargetBranchId,
        userId,
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(InventoryTransferResponseMapper.ToResponse(transfer));
  }
}
