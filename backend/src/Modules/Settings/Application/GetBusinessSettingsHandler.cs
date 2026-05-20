using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Settings.Contracts.Responses;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public sealed record GetBusinessSettingsQuery;

public sealed class GetBusinessSettingsHandler(
  IBusinessSettingsRepository repository,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result<BusinessSettingsResponse>> Handle(
    GetBusinessSettingsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessIdValue)
    {
      return Result.Failure<BusinessSettingsResponse>(SettingsErrors.UserContextRequired);
    }

    var businessId = new BusinessId(businessIdValue);
    var settings = await repository.GetAsync(businessId, cancellationToken)
      ?? BusinessSettings.Default(businessId, currentUser.UserId ?? Guid.Empty, clock.UtcNow);

    return Result.Success(ToResponse(settings));
  }

  private static BusinessSettingsResponse ToResponse(BusinessSettings s)
    => new(s.BusinessId.Value, s.CommercialName, s.LegalName, s.Rnc, s.Phone,
        s.Email, s.Address, s.Currency, s.Timezone, s.LogoUrl,
        s.ReceiptFooterText, s.UpdatedBy, s.UpdatedAt);
}
