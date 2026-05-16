using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class UpdateCurrentBusinessHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<CurrentBusinessResponse>> Handle(
    UpdateCurrentBusinessCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CurrentBusinessResponse>> HandleCoreAsync(
    UpdateCurrentBusinessCommand command,
    CancellationToken cancellationToken)
  {
    if (!currentUser.Roles.Contains("Admin", StringComparer.Ordinal))
    {
      return Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.Forbidden);
    }

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessIdValue)
    {
      return Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.InvalidCurrentUser);
    }

    if (!SettingsValidation.TryNormalizeBusiness(
          command.BusinessName,
          command.IdentificationType,
          command.IdentificationNumber,
          command.Phones,
          out var identificationType,
          out var identificationNumber,
          out var phones))
    {
      return Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.InvalidBusiness);
    }

    var businessId = new BusinessId(businessIdValue);

    if (await settings.ExistsBusinessIdentificationAsync(
          identificationType,
          identificationNumber,
          businessId,
          cancellationToken))
    {
      return Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.DuplicateIdentification);
    }

    var business = await settings.GetBusinessAsync(businessId, cancellationToken);

    if (business is null)
    {
      return Result.Failure<CurrentBusinessResponse>(IdentitySettingsErrors.BusinessNotFound);
    }

    business.UpdateDetails(command.BusinessName, identificationType, identificationNumber, clock.UtcNow);
    business.ReplacePhones(phones, clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SettingsResponseMapper.ToCurrentBusinessResponse(business));
  }
}
