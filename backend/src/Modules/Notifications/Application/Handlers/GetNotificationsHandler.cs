using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Application.Handlers;

public sealed record GetNotificationsQuery(
  Guid? BranchId = null,
  OperationalNotificationStatus? Status = null,
  OperationalNotificationType? Type = null,
  int Page = 1,
  int PageSize = 20);

public sealed record NotificationListResponse(
  IReadOnlyList<NotificationItemResponse> Items,
  int TotalCount,
  int Page,
  int PageSize,
  int UnreadCount);

public sealed record NotificationItemResponse(
  Guid Id,
  string Type,
  string Severity,
  string Status,
  string Title,
  string Message,
  Guid? BranchId,
  Guid? RelatedEntityId,
  string? RelatedEntityType,
  DateTimeOffset CreatedAt,
  DateTimeOffset? ReadAt);

public sealed class GetNotificationsHandler(
  IOperationalNotificationRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<NotificationListResponse>> Handle(
    GetNotificationsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
      return Result.Failure<NotificationListResponse>(NotificationErrors.UserContextRequired);

    var bid = new BusinessId(businessId);
    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);

    var (items, totalCount) = await repository.GetPagedAsync(
      bid, query.BranchId, query.Status, query.Type, page, pageSize, cancellationToken);

    var unreadCount = await repository.GetUnreadCountAsync(bid, cancellationToken);

    var responses = items.Select(MapToResponse).ToList();

    return Result.Success(new NotificationListResponse(responses, totalCount, page, pageSize, unreadCount));
  }

  private static NotificationItemResponse MapToResponse(OperationalNotification n)
    => new(
      n.Id,
      n.Type.ToString(),
      n.Severity.ToString(),
      n.Status.ToString(),
      n.Title,
      n.Message,
      n.BranchId,
      n.RelatedEntityId,
      n.RelatedEntityType,
      n.CreatedAt,
      n.ReadAt);
}
