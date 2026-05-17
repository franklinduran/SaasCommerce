using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfSaleRepository(AppDbContext dbContext) : ISaleRepository
{
  public Task<Sale?> GetAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<Sale>()
      .Include(sale => sale.Items)
      .SingleOrDefaultAsync(
        sale => sale.BusinessId == businessId && sale.Id == saleId,
        cancellationToken);

  public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(sale);

    await dbContext.Set<Sale>().AddAsync(sale, cancellationToken);
  }
}
