using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

public sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
  public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    => dbContext.SaveChangesAsync(cancellationToken);
}
