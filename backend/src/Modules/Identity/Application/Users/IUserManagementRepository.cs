using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public interface IUserManagementRepository
{
  Task<UserListResponse> GetUsersAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<User?> GetByIdInBusinessAsync(
    Guid userId,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<bool> HasOtherAdminOrOwnerAsync(
    Guid excludeUserId,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<bool> EmailExistsInBusinessAsync(
    string email,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task CreateUserAsync(
    User user,
    Role role,
    CancellationToken cancellationToken = default);

  Task<User?> GetByIdAsync(
    Guid userId,
    CancellationToken cancellationToken = default);
}
