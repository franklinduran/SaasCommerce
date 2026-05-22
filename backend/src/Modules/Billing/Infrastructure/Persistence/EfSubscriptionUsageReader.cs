using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence;

public sealed class EfSubscriptionUsageReader(AppDbContext context) : ISubscriptionUsageReader
{
  public Task<int> CountActiveBranchesAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => context.Set<Branch>()
      .AsNoTracking()
      .CountAsync(branch => branch.BusinessId == businessId && branch.IsActive, cancellationToken);

  public Task<int> CountActiveUsersAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => context.Set<User>()
      .AsNoTracking()
      .CountAsync(user => user.BusinessId == businessId && user.IsActive, cancellationToken);

  public Task<int> CountActiveProductsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => context.Set<Product>()
      .AsNoTracking()
      .CountAsync(product => product.BusinessId == businessId && product.IsActive, cancellationToken);

  public Task<int> CountMonthlySalesAsync(
    BusinessId businessId,
    DateTimeOffset monthStart,
    DateTimeOffset monthEnd,
    CancellationToken cancellationToken = default)
    => context.Set<Sale>()
      .AsNoTracking()
      .CountAsync(
        sale => sale.BusinessId == businessId &&
          sale.CreatedAt >= monthStart &&
          sale.CreatedAt < monthEnd,
        cancellationToken);
}
