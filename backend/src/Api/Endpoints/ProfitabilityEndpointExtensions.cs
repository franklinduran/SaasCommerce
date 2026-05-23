using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Sales.Application.Profitability;

namespace SaasCommerce.Api.Endpoints;

internal static class ProfitabilityEndpointExtensions
{
  private const string Tag = "Profitability";

  internal static WebApplication MapProfitabilityEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/profitability/summary",
      async (
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? branchId,
        GetProfitabilitySummaryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetProfitabilitySummaryQuery(from, to, branchId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProfitabilityView}")
      .WithTags(Tag);

    app.MapGet(
      "/api/profitability/products",
      async (
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? branchId,
        Guid? categoryId,
        GetProductProfitabilityHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetProductProfitabilityQuery(from, to, branchId, categoryId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProfitabilityProducts}")
      .WithTags(Tag);

    app.MapGet(
      "/api/profitability/branches",
      async (
        DateTimeOffset from,
        DateTimeOffset to,
        GetBranchProfitabilityHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetBranchProfitabilityQuery(from, to),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProfitabilityBranches}")
      .WithTags(Tag);

    app.MapGet(
      "/api/profitability/alerts",
      async (
        DateTimeOffset from,
        DateTimeOffset to,
        Guid? branchId,
        GetProfitabilityAlertsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetProfitabilityAlertsQuery(from, to, branchId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProfitabilityAlerts}")
      .WithTags(Tag);

    return app;
  }
}
