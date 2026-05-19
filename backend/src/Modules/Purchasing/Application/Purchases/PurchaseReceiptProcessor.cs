using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class PurchaseReceiptProcessor(
  IInventoryRepository inventory,
  IProductPurchaseReader products,
  IOutboxWriter outbox,
  IClock clock)
{
  public Task<Result> ProcessAsync(
    Purchase purchase,
    Guid userId,
    Guid correlationId,
    bool markAsReceived,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(purchase);

    return ProcessCoreAsync(purchase, userId, correlationId, markAsReceived, cancellationToken);
  }

  private async Task<Result> ProcessCoreAsync(
    Purchase purchase,
    Guid userId,
    Guid correlationId,
    bool markAsReceived,
    CancellationToken cancellationToken)
  {
    if (markAsReceived)
    {
      try
      {
        purchase.Receive(clock.UtcNow);
      }
      catch (InvalidOperationException)
      {
        return Result.Failure(PurchaseErrors.InvalidPurchaseState);
      }
    }
    else if (purchase.Status == PurchaseStatus.Cancelled || purchase.Status == PurchaseStatus.Draft)
    {
      return Result.Failure(PurchaseErrors.InvalidPurchaseState);
    }

    var businessId = purchase.BusinessId;
    var branchId = purchase.BranchId;

    if (await inventory.HasPurchaseMovementAsync(businessId, branchId, purchase.Id, cancellationToken))
    {
      return Result.Success();
    }

    foreach (var item in purchase.Items)
    {
      var result = await ProcessItemAsync(
        purchase,
        item,
        userId,
        correlationId,
        cancellationToken);

      if (result.IsFailure)
      {
        return result;
      }
    }

    return Result.Success();
  }

  private async Task<Result> ProcessItemAsync(
    Purchase purchase,
    PurchaseItem item,
    Guid userId,
    Guid correlationId,
    CancellationToken cancellationToken)
  {
    var product = await products.GetAsync(
      purchase.BusinessId.Value,
      item.ProductId,
      cancellationToken);

    if (product is null || !product.IsActive)
    {
      return Result.Failure(PurchaseErrors.ProductNotFound);
    }

    if (!product.TrackInventory)
    {
      return Result.Failure(PurchaseErrors.ProductDoesNotTrackInventory);
    }

    var stockItem = await inventory.GetStockItemAsync(
      purchase.BusinessId,
      purchase.BranchId,
      item.ProductId,
      cancellationToken);
    var isNewStockItem = stockItem is null;

    stockItem ??= new StockItem(
      Guid.NewGuid(),
      purchase.BusinessId,
      purchase.BranchId,
      item.ProductId,
      clock.UtcNow);

    var previousStock = stockItem.Quantity;
    var costUpdate = await products.UpdateAverageCostAsync(
      purchase.BusinessId.Value,
      item.ProductId,
      previousStock,
      item.Quantity,
      item.UnitCost,
      clock.UtcNow,
      cancellationToken);

    if (costUpdate is null)
    {
      return Result.Failure(PurchaseErrors.ProductNotFound);
    }

    var movement = stockItem.ApplyAdjustment(
      item.Quantity,
      InventoryMovementReason.PurchaseReceived,
      userId,
      product.AllowNegativeStock,
      clock.UtcNow,
      new InventoryMovementSource(PurchaseId: purchase.Id, Note: $"Purchase {purchase.Id:D}"));

    if (isNewStockItem)
    {
      await inventory.AddStockItemAsync(stockItem, cancellationToken);
    }

    await inventory.AddMovementAsync(movement, cancellationToken);
    await outbox.AddAsync(
      new InventoryIncreasedEventV1(
        Guid.NewGuid(),
        correlationId,
        purchase.Id,
        purchase.BusinessId.Value,
        purchase.BranchId.Value,
        item.ProductId,
        movement.Id,
        movement.PreviousStock,
        movement.NewStock,
        item.Quantity,
        clock.UtcNow),
      cancellationToken);
    await outbox.AddAsync(
      new ProductCostUpdatedEventV1(
        Guid.NewGuid(),
        correlationId,
        purchase.Id,
        purchase.BusinessId.Value,
        purchase.BranchId.Value,
        item.ProductId,
        costUpdate.PreviousCost,
        costUpdate.NewCost,
        clock.UtcNow),
      cancellationToken);

    return Result.Success();
  }
}
