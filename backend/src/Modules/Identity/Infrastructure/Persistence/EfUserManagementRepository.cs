using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Users;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfUserManagementRepository(AppDbContext dbContext) : IUserManagementRepository
{
  public async Task<UserListResponse> GetUsersAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var users = await dbContext.Set<User>()
      .AsNoTracking()
      .Include(u => u.Roles)
      .Where(u => u.BusinessId == businessId)
      .OrderBy(u => u.FullName)
      .ToListAsync(cancellationToken);

    var items = users
      .Select(u => new UserSummaryResponse(
        u.Id,
        u.FullName,
        u.Email,
        u.Phone,
        u.Roles.Select(r => r.Name).FirstOrDefault() ?? string.Empty,
        u.IsActive,
        u.CreatedAt))
      .ToArray();

    return new UserListResponse(items, items.Length);
  }

  public Task<User?> GetByIdInBusinessAsync(
    Guid userId,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<User>()
      .Include(u => u.Roles)
      .SingleOrDefaultAsync(
        u => u.Id == userId && u.BusinessId == businessId,
        cancellationToken);

  public Task<bool> HasOtherAdminOrOwnerAsync(
    Guid excludeUserId,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<User>()
      .Include(u => u.Roles)
      .Where(u =>
        u.BusinessId == businessId &&
        u.Id != excludeUserId &&
        u.IsActive &&
        u.Roles.Any(r => r.Name == SystemRoles.Admin || r.Name == SystemRoles.Owner))
      .AnyAsync(cancellationToken);
}
