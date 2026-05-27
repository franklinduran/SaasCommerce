using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Sales.Application.CashRegisters;
using SaasCommerce.Modules.Sales.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class CashRegisterEndpointExtensions
{
  private const string CashRegisterTag = "CashRegister";

  internal static WebApplication MapCashRegisterEndpoints(this WebApplication app)
  {
    app.MapPost(
      "/api/cash-registers/open",
      async (
        OpenCashRegisterRequest request,
        OpenCashRegisterHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new OpenCashRegisterCommand(request.BranchId, request.OpeningAmount, request.Notes),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashOpen}")
      .WithTags(CashRegisterTag);

    app.MapGet(
      "/api/cash-registers",
      async (
        [AsParameters] CashRegisterSearchParameters parameters,
        GetCashRegistersHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetCashRegistersQuery(
            parameters.BranchId,
            parameters.Status,
            parameters.DateFrom,
            parameters.DateTo,
            parameters.Page ?? 1,
            parameters.PageSize ?? 20),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashView}")
      .WithTags(CashRegisterTag);

    app.MapGet(
      "/api/cash-registers/active",
      async (
        GetActiveCashRegisterHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashView}")
      .WithTags(CashRegisterTag);

    app.MapGet(
      "/api/cash-registers/{id:guid}",
      async (
        Guid id,
        GetCashRegisterDetailHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetCashRegisterDetailQuery(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashView}")
      .WithTags(CashRegisterTag);

    app.MapPost(
      "/api/cash-registers/{id:guid}/movements",
      async (
        Guid id,
        RegisterCashRegisterMovementRequest request,
        RegisterCashRegisterMovementHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new RegisterCashRegisterMovementCommand(id, request.Type, request.Amount, request.Reason),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashRegisterMovement}")
      .WithTags(CashRegisterTag);

    app.MapPost(
      "/api/cash-registers/{id:guid}/close",
      async (
        Guid id,
        CloseCashRegisterRequest request,
        CloseCashRegisterHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CloseCashRegisterCommand(id, request.CountedAmount, request.CloseNotes),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashClose}")
      .WithTags(CashRegisterTag);

    app.MapGet(
      "/api/cash-registers/daily-summary",
      async (
        [AsParameters] DailySummaryParameters parameters,
        GetDailyCashRegisterSummaryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var summaryDate = parameters.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await handler.Handle(
          new GetDailyCashRegisterSummaryQuery(summaryDate, parameters.BranchId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashRegisterDailySummary}")
      .WithTags(CashRegisterTag);

    return app;
  }
}

internal sealed class DailySummaryParameters
{
  public DateOnly? Date { get; init; }
  public Guid? BranchId { get; init; }
}

internal sealed class CashRegisterSearchParameters
{
  public Guid? BranchId { get; init; }
  public string? Status { get; init; }
  public DateTimeOffset? DateFrom { get; init; }
  public DateTimeOffset? DateTo { get; init; }
  public int? Page { get; init; }
  public int? PageSize { get; init; }
}
