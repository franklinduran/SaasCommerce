using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Onboarding;

public sealed class GetOnboardingStatusHandler(
  IOnboardingStatusReader reader,
  ICurrentUserService currentUser)
{
  public async Task<Result<OnboardingStatusResponse>> Handle(
    CancellationToken cancellationToken = default)
  {
    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<OnboardingStatusResponse>(
        IdentityPermissionsErrors.UserContextRequired);
    }

    var summary = await reader.ReadAsync(
      new BusinessId(businessId),
      cancellationToken);

    return Result.Success(new OnboardingStatusResponse(
      summary.BusinessInfoCompleted,
      summary.ProductsCompleted,
      summary.InventoryCompleted,
      summary.CashSessionCompleted,
      summary.IsComplete,
      summary.CompletedCount,
      OnboardingStatusSummary.TotalSteps));
  }
}
