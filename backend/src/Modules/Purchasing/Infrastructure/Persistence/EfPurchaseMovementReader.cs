using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Infrastructure.Persistence;

public sealed class EfPurchaseMovementReader(AppDbContext dbContext) : IPurchaseMovementReader
{
  public async Task<IReadOnlyCollection<PurchaseInventoryMovementResponse>> ListByPurchaseAsync(
    BusinessId businessId,
    Guid purchaseId,
    CancellationToken cancellationToken = default)
    => await dbContext.Set<InventoryMovement>()
      .AsNoTracking()
      .Where(movement => movement.BusinessId == businessId && movement.PurchaseId == purchaseId)
      .OrderByDescending(movement => movement.CreatedAt)
      .Select(movement => new PurchaseInventoryMovementResponse(
        movement.Id,
        movement.ProductId,
        movement.PreviousStock,
        movement.NewStock,
        movement.Quantity,
        movement.Reason.ToString(),
        movement.CreatedAt))
      .ToArrayAsync(cancellationToken);
}
