using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Application.Handlers;

public sealed record MarkNotificationReadCommand(Guid NotificationId);

public sealed class MarkNotificationReadHandler(
  IOperationalNotificationRepository repository,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result> Handle(
    MarkNotificationReadCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId || currentUser.UserId is not Guid userId)
      return Result.Failure(NotificationErrors.UserContextRequired);

    var notification = await repository.GetByIdAsync(
      command.NotificationId,
      new BusinessId(businessId),
      cancellationToken);

    if (notification is null)
      return Result.Failure(NotificationErrors.NotFound);

    notification.MarkRead(userId, clock.UtcNow);

    await repository.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
