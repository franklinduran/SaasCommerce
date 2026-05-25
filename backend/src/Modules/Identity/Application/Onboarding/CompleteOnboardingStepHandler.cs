using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Onboarding;

public sealed class CompleteOnboardingStepHandler(
  IOnboardingStatusReader reader,
  ICurrentUserService currentUser)
{
  /// <summary>
  /// Marks an onboarding step as completed. Steps that are data-driven (Products, Inventory,
  /// CashSession) complete automatically when the related data exists.
  /// The handler always returns the current onboarding status after the operation.
  /// </summary>
  public async Task<Result<OnboardingStatusResponse>> Handle(
    string step,
    CancellationToken cancellationToken = default)
  {
    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<OnboardingStatusResponse>(
        IdentityPermissionsErrors.UserContextRequired);
    }

    if (!Enum.TryParse<OnboardingStep>(step, true, out _))
    {
      return Result.Failure<OnboardingStatusResponse>(
        new DomainError("ONBOARDING_INVALID_STEP",
          $"Invalid onboarding step '{step}'. Valid steps: BusinessInfo, Products, Inventory, CashSession."));
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
