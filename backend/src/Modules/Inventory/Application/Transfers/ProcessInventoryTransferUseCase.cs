using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public interface IProcessInventoryTransferUseCase
{
  Task<Result> ExecuteAsync(
    InventoryTransferRequestedEventV1 message,
    CancellationToken cancellationToken = default);
}

public sealed class ProcessInventoryTransferUseCase(
  IInventoryTransferRepository transferRepository,
  IInventoryRepository inventoryRepository,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IProcessInventoryTransferUseCase
{
  public Task<Result> ExecuteAsync(
    InventoryTransferRequestedEventV1 message,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(message);

    return ExecuteCoreAsync(message, cancellationToken);
  }

  private async Task<Result> ExecuteCoreAsync(
    InventoryTransferRequestedEventV1 message,
    CancellationToken cancellationToken)
  {
    var tenantId = new BusinessId(message.BusinessId);
    var transfer = await transferRepository.GetByIdAsync(tenantId, message.TransferId, cancellationToken);

    if (transfer is null || transfer.Status != InventoryTransferStatus.Pending)
    {
      return Result.Success();
    }

    var sourceBranchId = new BranchId(message.SourceBranchId);
    var targetBranchId = new BranchId(message.TargetBranchId);
    var now = clock.UtcNow;
    var systemUserId = message.CreatedByUserId;

    var sourceMovements = new List<InventoryMovement>();
    var targetMovements = new List<InventoryMovement>();
    var newTargetStockItems = new List<StockItem>();

    foreach (var item in transfer.Items)
    {
      var sourceStock = await inventoryRepository.GetStockItemAsync(
        tenantId, sourceBranchId, item.ProductId, cancellationToken);

      if (sourceStock is null || sourceStock.Quantity < item.Quantity)
      {
        var reason = sourceStock is null
          ? $"Product {item.ProductId} has no stock record in source branch."
          : $"Insufficient stock for product {item.ProductId}. Available: {sourceStock.Quantity}, Required: {item.Quantity}.";

        transfer.Fail(reason, now);

        await outbox.AddAsync(
          new InventoryTransferFailedEventV1(
            Guid.NewGuid(),
            message.CorrelationId,
            message.BusinessId,
            transfer.Id,
            message.SourceBranchId,
            message.TargetBranchId,
            reason,
            now),
          cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
      }

      var outMovement = sourceStock.ApplyAdjustment(
        -item.Quantity,
        InventoryMovementReason.TransferOut,
        systemUserId,
        allowNegativeStock: false,
        now,
        new InventoryMovementSource(Note: $"Transfer {transfer.Id}"));

      sourceMovements.Add(outMovement);

      var targetStock = await inventoryRepository.GetStockItemAsync(
        tenantId, targetBranchId, item.ProductId, cancellationToken);

      if (targetStock is null)
      {
        targetStock = new StockItem(Guid.NewGuid(), tenantId, targetBranchId, item.ProductId, now);
        newTargetStockItems.Add(targetStock);
      }

      var inMovement = targetStock.ApplyAdjustment(
        item.Quantity,
        InventoryMovementReason.TransferIn,
        systemUserId,
        allowNegativeStock: false,
        now,
        new InventoryMovementSource(Note: $"Transfer {transfer.Id}"));

      targetMovements.Add(inMovement);
    }

    foreach (var newItem in newTargetStockItems)
    {
      await inventoryRepository.AddStockItemAsync(newItem, cancellationToken);
    }

    foreach (var movement in sourceMovements.Concat(targetMovements))
    {
      await inventoryRepository.AddMovementAsync(movement, cancellationToken);
    }

    transfer.Complete(now);

    await outbox.AddAsync(
      new InventoryTransferCompletedEventV1(
        Guid.NewGuid(),
        message.CorrelationId,
        message.BusinessId,
        transfer.Id,
        message.SourceBranchId,
        message.TargetBranchId,
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }
}
