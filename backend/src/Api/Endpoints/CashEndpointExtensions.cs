using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Sales.Application.Cash;
using SaasCommerce.Modules.Sales.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class CashEndpointExtensions
{
  private const string CashTag = "Cash";

  internal static WebApplication MapCashEndpoints(this WebApplication app)
  {
    app.MapPost(
      "/api/cash-sessions",
      async (
        OpenCashSessionRequest request,
        OpenCashSessionHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new OpenCashSessionCommand(request.OpeningBalance, request.Notes),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashOpen}")
      .WithTags(CashTag);

    app.MapGet(
      "/api/cash-sessions/current",
      async (
        GetCurrentCashSessionHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashView}")
      .WithTags(CashTag);

    app.MapGet(
      "/api/cash-sessions",
      async (
        Guid? branchId,
        string? status,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        int? page,
        int? pageSize,
        GetCashSessionsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetCashSessionsQuery(branchId, status, dateFrom, dateTo, page ?? 1, pageSize ?? 20),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashView}")
      .WithTags(CashTag);

    app.MapGet(
      "/api/cash-sessions/{id:guid}",
      async (
        Guid id,
        GetCashSessionDetailHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetCashSessionDetailQuery(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashView}")
      .WithTags(CashTag);

    app.MapPost(
      "/api/cash-sessions/{id:guid}/close",
      async (
        Guid id,
        CloseCashSessionRequest request,
        CloseCashSessionHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CloseCashSessionCommand(id, request.ClosingBalance),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashClose}")
      .WithTags(CashTag);

    app.MapPost(
      "/api/cash-sessions/{id:guid}/movements",
      async (
        Guid id,
        RegisterCashMovementRequest request,
        RegisterCashMovementHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new RegisterCashMovementCommand(id, request.Type, request.Amount, request.Description),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashRegisterMovement}")
      .WithTags(CashTag);

    return app;
  }
}
