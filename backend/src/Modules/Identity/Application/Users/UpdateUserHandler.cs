using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed class UpdateUserHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IAuditLogWriter auditLogWriter)
{
  public async Task<Result> Handle(
    UpdateUserCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure(IdentityPermissionsErrors.UserContextRequired);
    }

    var businessIdVo = new BusinessId(businessId);
    var user = await repository.GetByIdInBusinessAsync(
      command.UserId,
      businessIdVo,
      cancellationToken);

    if (user is null)
    {
      return Result.Failure(IdentityPermissionsErrors.UserNotFound);
    }

    var now = clock.UtcNow;
    user.UpdateProfile(command.FullName, command.Phone, now);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    // Create audit log entry
    var auditEntry = new AuditEntry(
      businessIdVo,
      currentUser.UserId,
      "user.updated",
      "User",
      command.UserId,
      $"Updated user profile: {command.FullName}");

    await auditLogWriter.WriteAsync(auditEntry, cancellationToken);

    return Result.Success();
  }
}
