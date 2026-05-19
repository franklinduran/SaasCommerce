using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class DeductSaleInventoryUseCase(
  IProductSalesPolicyReader productPolicies,
  IInventoryAvailabilityService inventoryAvailability,
  IInventoryRepository inventory,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IDeductSaleInventoryUseCase
{
  public async Task<Result> ExecuteAsync(
    InventoryDeductionRequestedEventV1 inventoryDeductionRequested,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(inventoryDeductionRequested);

    var workItems = await BuildDeductionWorkItemsAsync(
      inventoryDeductionRequested,
      cancellationToken);

    if (workItems.FailureReason is not null)
    {
      await PublishFailedAsync(
        inventoryDeductionRequested,
        workItems.FailureReason,
        cancellationToken);
      return Result.Success();
    }

    foreach (var workItem in workItems.Items)
    {
      var movement = workItem.StockItem.ApplyAdjustment(
        -workItem.SaleItem.Quantity,
        InventoryMovementReason.SaleDeduction,
        inventoryDeductionRequested.UserId,
        workItem.AllowNegativeStock,
        clock.UtcNow,
        new InventoryMovementSource(SaleId: inventoryDeductionRequested.SaleId, Note: $"Sale {inventoryDeductionRequested.SaleId:D}"));

      await inventory.AddMovementAsync(movement, cancellationToken);

      if (workItem.StockItem.IsLowStock(workItem.MinimumStock) &&
          workItem.MinimumStock is decimal minimumStock)
      {
        await outbox.AddAsync(
          new LowStockDetectedEventV1(
            Guid.NewGuid(),
            inventoryDeductionRequested.CorrelationId,
            inventoryDeductionRequested.BusinessId,
            inventoryDeductionRequested.BranchId,
            workItem.SaleItem.ProductId,
            workItem.ProductName,
            workItem.StockItem.Quantity,
            minimumStock,
            clock.UtcNow),
          cancellationToken);
      }
    }

    await outbox.AddAsync(
      new InventoryDeductedEventV1(
        Guid.NewGuid(),
        inventoryDeductionRequested.CorrelationId,
        inventoryDeductionRequested.SaleId,
        inventoryDeductionRequested.BusinessId,
        inventoryDeductionRequested.BranchId,
        inventoryDeductionRequested.UserId,
        inventoryDeductionRequested.Items,
        inventoryDeductionRequested.Total,
        inventoryDeductionRequested.PaymentMethod,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  private async Task<DeductionWorkItems> BuildDeductionWorkItemsAsync(
    InventoryDeductionRequestedEventV1 request,
    CancellationToken cancellationToken)
  {
    if (request.Items.Count == 0)
    {
      return DeductionWorkItems.Failed("Sale has no items.");
    }

    var businessId = new BusinessId(request.BusinessId);
    var branchId = new BranchId(request.BranchId);

    if (await inventory.HasSaleMovementAsync(
        businessId,
        branchId,
        request.SaleId,
        cancellationToken))
    {
      return DeductionWorkItems.Success([]);
    }

    var workItems = new List<DeductionWorkItem>();

    foreach (var item in request.Items)
    {
      var policy = await productPolicies.GetSalesPolicyAsync(
        request.BusinessId,
        item.ProductId,
        cancellationToken);

      if (policy is null)
      {
        return DeductionWorkItems.Failed($"Product {item.ProductId} was not found.");
      }

      if (!policy.CanBeSold)
      {
        return DeductionWorkItems.Failed(policy.ReasonIfCannotBeSold ?? $"Product {item.ProductId} cannot be sold.");
      }

      if (!policy.TrackInventory)
      {
        continue;
      }

      var availability = await inventoryAvailability.ValidateStockAsync(
        new InventoryAvailabilityRequest(
          request.BusinessId,
          request.BranchId,
          item.ProductId,
          item.Quantity,
          policy.TrackInventory,
          policy.AllowNegativeStock),
        cancellationToken);

      if (!availability.IsAvailable)
      {
        return DeductionWorkItems.Failed(
          availability.ReasonIfUnavailable ?? $"Product {item.ProductId} does not have enough stock.");
      }

      var stockItem = await inventory.GetStockItemAsync(
        businessId,
        branchId,
        item.ProductId,
        cancellationToken);

      if (stockItem is null)
      {
        stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, item.ProductId, clock.UtcNow);
        await inventory.AddStockItemAsync(stockItem, cancellationToken);
      }

      workItems.Add(new DeductionWorkItem(
        stockItem,
        item,
        policy.AllowNegativeStock,
        policy.MinimumStock,
        policy.Name));
    }

    return DeductionWorkItems.Success(workItems);
  }

  private async Task PublishFailedAsync(
    InventoryDeductionRequestedEventV1 request,
    string reason,
    CancellationToken cancellationToken)
  {
    await outbox.AddAsync(
      new InventoryDeductionFailedEventV1(
        Guid.NewGuid(),
        request.CorrelationId,
        request.SaleId,
        request.BusinessId,
        request.BranchId,
        request.UserId,
        reason,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }

  private sealed record DeductionWorkItem(
    StockItem StockItem,
    SaleItemV1 SaleItem,
    bool AllowNegativeStock,
    decimal? MinimumStock,
    string ProductName);

  private sealed record DeductionWorkItems(
    IReadOnlyCollection<DeductionWorkItem> Items,
    string? FailureReason)
  {
    public static DeductionWorkItems Success(IReadOnlyCollection<DeductionWorkItem> items) => new(items, null);

    public static DeductionWorkItems Failed(string failureReason) => new([], failureReason);
  }
}
