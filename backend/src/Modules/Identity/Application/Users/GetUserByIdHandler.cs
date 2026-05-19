using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed class GetUserByIdHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<UserDetailResponse>> Handle(
    GetUserByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<UserDetailResponse>(IdentityPermissionsErrors.UserContextRequired);
    }

    var businessIdVo = new BusinessId(businessId);
    var user = await repository.GetByIdInBusinessAsync(
      query.UserId,
      businessIdVo,
      cancellationToken);

    if (user is null)
    {
      return Result.Failure<UserDetailResponse>(IdentityPermissionsErrors.UserNotFound);
    }

    var roleName = user.Roles.FirstOrDefault()?.Name ?? "Unknown";

    var response = new UserDetailResponse(
      user.Id,
      user.FullName,
      user.Email,
      user.Phone,
      roleName,
      user.DefaultBranchId?.Value,
      user.IsActive,
      user.MustChangePassword,
      user.CreatedAt,
      user.UpdatedAt);

    return Result.Success(response);
  }
}
