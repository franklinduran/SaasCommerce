using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Settings.Contracts.Responses;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public sealed record GetBillingSettingsQuery;

public sealed class GetBillingSettingsHandler(
  IBillingSettingsRepository repository,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result<BillingSettingsResponse>> Handle(
    GetBillingSettingsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessIdValue)
    {
      return Result.Failure<BillingSettingsResponse>(SettingsErrors.UserContextRequired);
    }

    var businessId = new BusinessId(businessIdValue);
    var settings = await repository.GetAsync(businessId, cancellationToken)
      ?? BillingSettings.Default(businessId, currentUser.UserId ?? Guid.Empty, clock.UtcNow);

    return Result.Success(ToResponse(settings));
  }

  private static BillingSettingsResponse ToResponse(BillingSettings s)
    => new(s.BusinessId.Value, s.ReceiptHeaderText, s.ReceiptFooterText,
        s.ShowLogoOnReceipt, s.ShowRncOnReceipt, s.EnableInvoiceAutoGeneration,
        s.InvoicePrefix, s.InvoiceSequenceStart, s.UpdatedBy, s.UpdatedAt);
}
