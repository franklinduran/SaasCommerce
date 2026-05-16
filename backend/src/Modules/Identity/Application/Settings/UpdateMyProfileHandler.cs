using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class UpdateMyProfileHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<CurrentUserResponse>> Handle(
    UpdateMyProfileCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CurrentUserResponse>> HandleCoreAsync(
    UpdateMyProfileCommand command,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(command.FullName))
    {
      return Result.Failure<CurrentUserResponse>(IdentitySettingsErrors.InvalidProfile);
    }

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

    user.UpdateProfile(command.FullName, command.Phone, clock.UtcNow);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    var business = await settings.GetBusinessAsync(new SharedKernel.Tenancy.BusinessId(businessId.Value), cancellationToken);
    var branch = currentUser.BranchId is { } branchId
      ? await settings.GetBranchAsync(
        new SharedKernel.Tenancy.BusinessId(businessId.Value),
        new SharedKernel.Tenancy.BranchId(branchId),
        cancellationToken)
      : null;

    return business is null
      ? Result.Failure<CurrentUserResponse>(IdentitySettingsErrors.BusinessNotFound)
      : Result.Success(SettingsResponseMapper.ToCurrentUserResponse(user, business, branch));
  }
}
