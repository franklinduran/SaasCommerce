using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Sales.Application.DailyClosings;

namespace SaasCommerce.Api.Endpoints;

internal static class DailyClosingEndpointExtensions
{
  private const string Tag = "DailyClosing";

  internal static WebApplication MapDailyClosingEndpoints(this WebApplication app)
  {
    // Preview (read-only, no persistence)
    app.MapGet(
      "/api/daily-closing/preview",
      async (
        DateOnly date,
        Guid branchId,
        PreviewDailyClosingHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new PreviewDailyClosingQuery(date, branchId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.DailyClosingPreview}")
      .WithTags(Tag);

    // Create draft closing
    app.MapPost(
      "/api/daily-closing",
      async (
        CreateDailyClosingRequest request,
        CreateDailyClosingHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateDailyClosingCommand(request.Date, request.BranchId, request.Notes),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.DailyClosingCreate}")
      .WithTags(Tag);

    // Close a draft closing
    app.MapPost(
      "/api/daily-closing/{id:guid}/close",
      async (
        Guid id,
        CloseDailyClosingRequest request,
        CloseDailyClosingHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CloseDailyClosingCommand(id, request.CashCounted, request.Notes),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.DailyClosingClose}")
      .WithTags(Tag);

    // List closings (with pagination)
    app.MapGet(
      "/api/daily-closing",
      async (
        [AsParameters] DailyClosingListParameters parameters,
        GetDailyClosingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetDailyClosingsQuery(
            parameters.BranchId,
            parameters.DateFrom,
            parameters.DateTo,
            parameters.Page,
            parameters.PageSize),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.DailyClosingHistory}")
      .WithTags(Tag);

    // Get detail
    app.MapGet(
      "/api/daily-closing/{id:guid}",
      async (
        Guid id,
        GetDailyClosingDetailHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetDailyClosingDetailQuery(id),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.DailyClosingView}")
      .WithTags(Tag);

    return app;
  }
}

internal sealed record CreateDailyClosingRequest(DateOnly Date, Guid BranchId, string? Notes);

internal sealed record CloseDailyClosingRequest(decimal CashCounted, string? Notes);

internal sealed class DailyClosingListParameters
{
  public Guid? BranchId { get; init; }
  public DateOnly? DateFrom { get; init; }
  public DateOnly? DateTo { get; init; }
  public int Page { get; init; }
  public int PageSize { get; init; }
}
