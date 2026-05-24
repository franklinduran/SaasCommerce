using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Application.Handlers;

public sealed record GetUnreadCountQuery;

public sealed record UnreadCountResponse(int UnreadCount);

public sealed class GetUnreadCountHandler(
  IOperationalNotificationRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<UnreadCountResponse>> Handle(
    GetUnreadCountQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
      return Result.Failure<UnreadCountResponse>(NotificationErrors.UserContextRequired);

    var count = await repository.GetUnreadCountAsync(new BusinessId(businessId), cancellationToken);

    return Result.Success(new UnreadCountResponse(count));
  }
}
