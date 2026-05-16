using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class AdjustInventoryHandler(
  IInventoryRepository inventory,
  IProductInventoryPolicyReader productPolicies,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<InventoryAdjustmentResponse>> Handle(
    AdjustInventoryCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<InventoryAdjustmentResponse>> HandleCoreAsync(
    AdjustInventoryCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.BranchId is not Guid branchId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.UserContextRequired);
    }

    if (command.ProductId == Guid.Empty ||
        command.Quantity == 0 ||
        !Enum.TryParse<InventoryMovementReason>(command.Reason, true, out var reason))
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.InvalidAdjustment);
    }

    var tenantId = new BusinessId(businessId);
    var currentBranchId = new BranchId(branchId);
    var productPolicy = await productPolicies.GetAsync(businessId, command.ProductId, cancellationToken);

    if (productPolicy is null)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.ProductNotFound);
    }

    if (!productPolicy.TrackInventory)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.ProductDoesNotTrackInventory);
    }

    var stockItem = await inventory.GetStockItemAsync(
      tenantId,
      currentBranchId,
      command.ProductId,
      cancellationToken);
    var isNewStockItem = stockItem is null;

    stockItem ??= new StockItem(Guid.NewGuid(), tenantId, currentBranchId, command.ProductId, clock.UtcNow);

    InventoryMovement movement;

    try
    {
      movement = stockItem.ApplyAdjustment(
        command.Quantity,
        reason,
        userId,
        productPolicy.AllowNegativeStock,
        clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.NegativeStock);
    }

    if (isNewStockItem)
    {
      await inventory.AddStockItemAsync(stockItem, cancellationToken);
    }

    await inventory.AddMovementAsync(movement, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new InventoryAdjustmentResponse(
      InventoryResponseMapper.ToResponse(stockItem, null),
      InventoryResponseMapper.ToResponse(movement)));
  }
}
