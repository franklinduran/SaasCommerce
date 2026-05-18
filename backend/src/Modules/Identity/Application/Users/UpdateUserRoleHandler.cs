using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record UpdateUserRoleCommand(Guid TargetUserId, string NewRole);

public sealed class UpdateUserRoleHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result> Handle(
    UpdateUserRoleCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure(IdentityPermissionsErrors.UserContextRequired);
    }

    if (!SystemRoles.All.Contains(command.NewRole))
    {
      return Result.Failure(new DomainError("identity.invalid_role", $"Role '{command.NewRole}' is not valid."));
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

    // Prevent removing the last admin/owner
    var isCurrentlyAdminOrOwner = user.Roles.Any(r =>
      r.Name is SystemRoles.Owner or SystemRoles.Admin);

    var newRoleIsAdminOrOwner = command.NewRole is SystemRoles.Owner or SystemRoles.Admin;

    if (isCurrentlyAdminOrOwner && !newRoleIsAdminOrOwner)
    {
      var hasOther = await repository.HasOtherAdminOrOwnerAsync(
        command.TargetUserId,
        businessIdVo,
        cancellationToken);

      if (!hasOther)
      {
        return Result.Failure(IdentityPermissionsErrors.CannotRemoveLastOwner);
      }
    }

    var newRole = new Role(Guid.NewGuid(), businessIdVo, command.NewRole);
    user.ChangeRole(newRole, clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
