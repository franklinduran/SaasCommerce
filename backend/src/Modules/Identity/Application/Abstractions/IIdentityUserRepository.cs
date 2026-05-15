using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Application.Abstractions;

public interface IIdentityUserRepository
{
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

  Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
