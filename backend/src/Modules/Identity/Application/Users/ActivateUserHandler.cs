using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Events.V1;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed class ActivateUserHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IAuditLogWriter auditLogWriter,
  IEventBus eventBus)
{
  public async Task<Result> Handle(
    ActivateUserCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure(IdentityPermissionsErrors.UserContextRequired);
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

    if (user.IsActive)
    {
      return Result.Failure(IdentityPermissionsErrors.UserAlreadyActive);
    }

    var now = clock.UtcNow;
    user.Activate(now);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    // Create audit log entry
    var auditEntry = new AuditEntry(
      businessIdVo,
      currentUser.UserId,
      "user.activated",
      "User",
      command.TargetUserId,
      "User activated");

    await auditLogWriter.WriteAsync(auditEntry, cancellationToken);

    // Publish integration event
    var integrationEvent = new UserActivatedIntegrationEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      businessId,
      command.TargetUserId,
      now);

    await eventBus.PublishAsync(integrationEvent, cancellationToken);

    return Result.Success();
  }
}
