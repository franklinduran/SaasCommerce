using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record DisableUserCommand(Guid TargetUserId);

public sealed class DisableUserHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result> Handle(
    DisableUserCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure(IdentityPermissionsErrors.UserContextRequired);
    }

    if (currentUser.UserId == command.TargetUserId)
    {
      return Result.Failure(IdentityPermissionsErrors.CannotDisableSelf);
    }

    var businessIdVo = new BusinessId(businessId);
    var user = await repository.GetByIdInBusinessAsync(
      command.TargetUserId,
      businessIdVo,
      cancellationToken);

    if (user is null)
    {
      return Result.Failure(IdentityPermissionsErrors.UserNotFound);
    }

    user.Deactivate(clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
