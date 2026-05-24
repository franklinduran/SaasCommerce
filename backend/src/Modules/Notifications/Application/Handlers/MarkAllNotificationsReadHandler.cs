using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Application.Handlers;

public sealed record MarkAllNotificationsReadCommand;

public sealed class MarkAllNotificationsReadHandler(
  IOperationalNotificationRepository repository,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result> Handle(
    MarkAllNotificationsReadCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId || currentUser.UserId is not Guid userId)
      return Result.Failure(NotificationErrors.UserContextRequired);

    var bid = new BusinessId(businessId);
    var (items, _) = await repository.GetPagedAsync(
      bid,
      branchId: null,
      status: OperationalNotificationStatus.Unread,
      type: null,
      page: 1,
      pageSize: 500,
      cancellationToken);

    var now = clock.UtcNow;
    foreach (var n in items)
    {
      n.MarkRead(userId, now);
    }

    await repository.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
