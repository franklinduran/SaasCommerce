using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Settings.Contracts.Responses;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public sealed record GetInventorySettingsQuery;

public sealed class GetInventorySettingsHandler(
  IInventorySettingsRepository repository,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result<InventorySettingsResponse>> Handle(
    GetInventorySettingsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessIdValue)
    {
      return Result.Failure<InventorySettingsResponse>(SettingsErrors.UserContextRequired);
    }

    var businessId = new BusinessId(businessIdValue);
    var settings = await repository.GetAsync(businessId, cancellationToken)
      ?? InventorySettings.Default(businessId, currentUser.UserId ?? Guid.Empty, clock.UtcNow);

    return Result.Success(ToResponse(settings));
  }

  private static InventorySettingsResponse ToResponse(InventorySettings s)
    => new(s.BusinessId.Value, s.EnableLowStockAlerts, s.DefaultLowStockThreshold,
        s.RequireReasonForInventoryAdjustment, s.AllowInventoryTransferBetweenBranches,
        s.UpdatedBy, s.UpdatedAt);
}
