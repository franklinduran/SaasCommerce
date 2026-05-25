using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Application.PilotBusiness;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class AdminEndpointExtensions
{
  private const string AdminTag = "Admin";

  internal static WebApplication MapAdminEndpoints(this WebApplication app)
  {
    app.MapPost(
      "/api/admin/pilot-businesses",
      async (
        CreatePilotBusinessRequest request,
        CreatePilotBusinessHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreatePilotBusinessCommand(
            request.BusinessName,
            request.IdentificationType,
            request.IdentificationNumber,
            request.Phone,
            request.BranchName,
            request.AdminFullName,
            request.AdminEmail,
            request.AdminPassword),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider,
          successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SaasManageBusinesses}")
      .WithTags(AdminTag);

    app.MapGet(
      "/api/admin/pilot-metrics",
      async (
        GetPilotMetricsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetPilotMetricsQuery(), cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SaasPilotMetrics}")
      .WithTags(AdminTag);

    return app;
  }
}
