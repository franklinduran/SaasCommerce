using SaasCommerce.Api;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Application.Audit;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Api.Endpoints;

internal static class AuditEndpointExtensions
{
  private const string AuditTag = "Audit";

  internal static WebApplication MapAuditEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/audit-logs",
      async (
        [AsParameters] AuditLogEndpointRequest request,
        GetAuditLogsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetAuditLogsQuery(request.DateFrom, request.DateTo, request.UserId, request.Action, request.EntityName, request.Page ?? 1, request.PageSize ?? 25),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.AuditView}")
      .WithTags(AuditTag);

    app.MapGet(
      "/api/audit-logs/{id:guid}",
      async (
        Guid id,
        GetAuditLogByIdHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetAuditLogByIdQuery(id), cancellationToken);

        if (result.IsFailure && result.Error == SaasCommerce.Modules.Identity.Application.Permissions.IdentityPermissionsErrors.UserNotFound)
        {
          return Results.NotFound();
        }

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.AuditView}")
      .WithTags(AuditTag);

    return app;
  }
}
