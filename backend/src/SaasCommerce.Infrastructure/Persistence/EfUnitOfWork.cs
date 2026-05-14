using SaasCommerce.Application.Abstractions.Persistence;

namespace SaasCommerce.Infrastructure.Persistence;

public sealed class EfUnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
  public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    => dbContext.SaveChangesAsync(cancellationToken);
}
