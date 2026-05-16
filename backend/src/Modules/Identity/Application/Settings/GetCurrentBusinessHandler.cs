using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class GetCurrentBusinessHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings)
{
  public async Task<Result<CurrentBusinessResponse>> Handle(
    CancellationToken cancellationToken = default)
  {
    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.InvalidCurrentUser);
    }

    var business = await settings.GetBusinessAsync(new BusinessId(businessId), cancellationToken);

    return business is null
      ? Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.BusinessNotFound)
      : Result.Success(SettingsResponseMapper.ToCurrentBusinessResponse(business));
  }
}
