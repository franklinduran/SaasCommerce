using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class RestoreInventoryFromSaleReturnUseCase(
  IInventoryRepository inventory,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IRestoreInventoryFromSaleReturnUseCase
{
  public async Task<Result> ExecuteAsync(
    SaleReturnApprovedEventV1 message,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(message);

    var businessId = new BusinessId(message.BusinessId);
    var branchId = new BranchId(message.BranchId);

    if (await inventory.HasSaleReturnMovementAsync(
        businessId,
        branchId,
        message.SaleReturnId,
        cancellationToken))
    {
      return Result.Success();
    }

    foreach (var item in message.Items)
    {
      var stockItem = await inventory.GetStockItemAsync(
        businessId,
        branchId,
        item.ProductId,
        cancellationToken);
      var isNew = stockItem is null;
      stockItem ??= new StockItem(Guid.NewGuid(), businessId, branchId, item.ProductId, clock.UtcNow);

      var movement = stockItem.ApplyAdjustment(
        item.Quantity,
        InventoryMovementReason.Return,
        message.UserId,
        allowNegativeStock: true,
        clock.UtcNow,
        new InventoryMovementSource(
          SaleId: message.SaleId,
          ReturnId: message.SaleReturnId,
          Note: $"Sale return {message.SaleReturnId:D}"));

      if (isNew)
      {
        await inventory.AddStockItemAsync(stockItem, cancellationToken);
      }

      await inventory.AddMovementAsync(movement, cancellationToken);
    }

    await outbox.AddAsync(
      new InventoryRestoredFromReturnEventV1(
        Guid.NewGuid(),
        message.CorrelationId,
        message.BusinessId,
        message.BranchId,
        message.SaleId,
        message.SaleReturnId,
        message.UserId,
        message.Items,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
