using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;

namespace SaasCommerce.Api.Endpoints;

internal static class NotificationEndpointExtensions
{
  private const string Tag = "Notifications";

  internal static WebApplication MapNotificationEndpoints(this WebApplication app)
  {
    // GET /api/notifications — paginated list
    app.MapGet(
      "/api/notifications",
      async (
        Guid? branchId,
        string? status,
        string? type,
        int page,
        int pageSize,
        GetNotificationsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        OperationalNotificationStatus? parsedStatus = Enum.TryParse<OperationalNotificationStatus>(status, out var s) ? s : null;
        OperationalNotificationType? parsedType = Enum.TryParse<OperationalNotificationType>(type, out var t) ? t : null;

        var result = await handler.Handle(
          new GetNotificationsQuery(branchId, parsedStatus, parsedType, page, pageSize),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.NotificationsView}")
      .WithTags(Tag);

    // GET /api/notifications/unread-count — for bell icon
    app.MapGet(
      "/api/notifications/unread-count",
      async (
        GetUnreadCountHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetUnreadCountQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.NotificationsView}")
      .WithTags(Tag);

    // PUT /api/notifications/{id}/read — mark one as read
    app.MapPut(
      "/api/notifications/{id:guid}/read",
      async (
        Guid id,
        MarkNotificationReadHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new MarkNotificationReadCommand(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.NotificationsRead}")
      .WithTags(Tag);

    // PUT /api/notifications/read-all — mark all as read
    app.MapPut(
      "/api/notifications/read-all",
      async (
        MarkAllNotificationsReadHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new MarkAllNotificationsReadCommand(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.NotificationsRead}")
      .WithTags(Tag);

    return app;
  }
}
