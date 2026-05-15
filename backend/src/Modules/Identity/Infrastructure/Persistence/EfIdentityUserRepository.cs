using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfIdentityUserRepository(AppDbContext dbContext) : IIdentityUserRepository
{
  public Task<User?> GetByEmailAsync(
    string email,
    CancellationToken cancellationToken = default)
    => Users()
      .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

  public Task<User?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    => Users()
      .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

  public Task<User?> GetByRefreshTokenAsync(
    string refreshToken,
    CancellationToken cancellationToken = default)
    => Users()
      .SingleOrDefaultAsync(
        user => user.RefreshTokens.Any(token => token.Token == refreshToken),
        cancellationToken);

  private IQueryable<User> Users()
    => dbContext.Set<User>()
      .Include(user => user.Roles)
      .Include(user => user.RefreshTokens);
}
