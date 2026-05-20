using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Settings.Contracts.Responses;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public sealed record GetSalesSettingsQuery;

public sealed class GetSalesSettingsHandler(
  ISalesSettingsRepository repository,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result<SalesSettingsResponse>> Handle(
    GetSalesSettingsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessIdValue)
    {
      return Result.Failure<SalesSettingsResponse>(SettingsErrors.UserContextRequired);
    }

    var businessId = new BusinessId(businessIdValue);
    var settings = await repository.GetAsync(businessId, cancellationToken)
      ?? SalesSettings.Default(businessId, currentUser.UserId ?? Guid.Empty, clock.UtcNow);

    return Result.Success(ToResponse(settings));
  }

  private static SalesSettingsResponse ToResponse(SalesSettings s)
    => new(s.BusinessId.Value, s.AllowNegativeStock, s.AllowDiscounts,
        s.RequireCustomerForCreditSale, s.DefaultPaymentMethod,
        s.EnableReceiptPrintAfterSale, s.EnableInvoiceAutoGeneration,
        s.UpdatedBy, s.UpdatedAt);
}
