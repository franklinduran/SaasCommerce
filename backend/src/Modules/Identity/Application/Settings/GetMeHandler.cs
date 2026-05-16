using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class GetMeHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings)
{
  public async Task<Result<CurrentUserResponse>> Handle(
    CancellationToken cancellationToken = default)
  {
    var userId = currentUser.UserId;
    var businessId = currentUser.BusinessId;

    if (!currentUser.IsAuthenticated || userId is null || businessId is null)
    {
      return Result.Failure<CurrentUserResponse>(IdentitySettingsErrors.InvalidCurrentUser);
    }

    var user = await settings.GetUserAsync(userId.Value, cancellationToken);

    if (user is null)
    {
      return Result.Failure<CurrentUserResponse>(IdentitySettingsErrors.UserNotFound);
    }

    var business = await settings.GetBusinessAsync(new BusinessId(businessId.Value), cancellationToken);

    if (business is null)
    {
      return Result.Failure<CurrentUserResponse>(IdentitySettingsErrors.BusinessNotFound);
    }

    var branch = currentUser.BranchId is { } branchId
      ? await settings.GetBranchAsync(
        new BusinessId(businessId.Value),
        new BranchId(branchId),
        cancellationToken)
      : null;

    return Result.Success(SettingsResponseMapper.ToCurrentUserResponse(user, business, branch));
  }
}
