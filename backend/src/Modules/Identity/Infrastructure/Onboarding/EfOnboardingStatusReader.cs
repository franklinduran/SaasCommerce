using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Identity.Application.Onboarding;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Onboarding;

public sealed class EfOnboardingStatusReader(AppDbContext dbContext) : IOnboardingStatusReader
{
  public async Task<OnboardingStatusSummary> ReadAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var hasProducts = await dbContext.Set<Product>()
      .AnyAsync(p => p.BusinessId == businessId && p.IsActive, cancellationToken);

    var hasInventory = await dbContext.Set<StockItem>()
      .AnyAsync(s => s.BusinessId == businessId && s.Quantity > 0, cancellationToken);

    var hasCashSession = await dbContext.Set<CashSession>()
      .AnyAsync(c => c.BusinessId == businessId, cancellationToken);

    // BusinessInfo is always considered completed once they're authenticated
    return new OnboardingStatusSummary(
      BusinessInfoCompleted: true,
      ProductsCompleted: hasProducts,
      InventoryCompleted: hasInventory,
      CashSessionCompleted: hasCashSession);
  }
}
