using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record GetUsersQuery;

public sealed class GetUsersHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<UserListResponse>> Handle(
    GetUsersQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<UserListResponse>(IdentityPermissionsErrors.UserContextRequired);
    }

    var response = await repository.GetUsersAsync(
      new BusinessId(businessId),
      cancellationToken);

    return Result.Success(response);
  }
}
