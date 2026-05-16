using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class GetCurrentBranchHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings)
{
  public async Task<Result<CurrentBranchResponse>> Handle(
    CancellationToken cancellationToken = default)
  {
    if (!currentUser.IsAuthenticated ||
        currentUser.BusinessId is not { } businessId ||
        currentUser.BranchId is not { } branchId)
    {
      return Result.Failure<CurrentBranchResponse>(IdentitySettingsErrors.InvalidCurrentUser);
    }

    var branch = await settings.GetBranchAsync(
      new BusinessId(businessId),
      new BranchId(branchId),
      cancellationToken);

    return branch is null
      ? Result.Failure<CurrentBranchResponse>(IdentitySettingsErrors.BranchNotFound)
      : Result.Success(SettingsResponseMapper.ToCurrentBranchResponse(branch));
  }
}
