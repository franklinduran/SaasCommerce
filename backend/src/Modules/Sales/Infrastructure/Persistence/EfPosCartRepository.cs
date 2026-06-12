using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfPosCartRepository(AppDbContext db) : IPosCartRepository
{
  public Task<PosCart?> GetAsync(BusinessId businessId, Guid userId, CancellationToken ct = default)
    => db.Set<PosCart>()
        .Include(c => c.Items)
        .FirstOrDefaultAsync(
          c => c.BusinessId == businessId && c.UserId == userId,
          ct);

  public async Task AddAsync(PosCart cart, CancellationToken ct = default)
    => await db.Set<PosCart>().AddAsync(cart, ct);

  public Task RemoveItemAsync(PosCartItem item, CancellationToken ct = default)
  {
    db.Set<PosCartItem>().Remove(item);
    return Task.CompletedTask;
  }
}
