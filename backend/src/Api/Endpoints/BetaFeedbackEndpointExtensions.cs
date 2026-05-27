using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Feedback.Application;
using SaasCommerce.Modules.Feedback.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Api.Endpoints;

internal static class BetaFeedbackEndpointExtensions
{
  private const string Tag = "Beta Feedback";

  internal static WebApplication MapBetaFeedbackEndpoints(this WebApplication app)
  {
    app.MapPost(
      "/api/beta/feedback",
      async (
        CreateBetaFeedbackRequest request,
        CreateBetaFeedbackHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateBetaFeedbackCommand(
            request.Category,
            request.Title,
            request.Description,
            request.ContextUrl),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BetaFeedbackCreate}")
      .WithTags(Tag);

    app.MapGet(
      "/api/beta/feedback",
      async (
        [AsParameters] BetaFeedbackListParameters parameters,
        GetBetaFeedbackHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetBetaFeedbackQuery(
            parameters.Status,
            parameters.Category,
            parameters.Page ?? 1,
            parameters.PageSize ?? 20),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BetaFeedbackView}")
      .WithTags(Tag);

    app.MapPut(
      "/api/beta/feedback/{id:guid}/status",
      async (
        Guid id,
        UpdateBetaFeedbackStatusRequest request,
        UpdateBetaFeedbackStatusHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateBetaFeedbackStatusCommand(id, request.Status, request.ReviewNote),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BetaFeedbackManage}")
      .WithTags(Tag);

    return app;
  }
}

internal sealed class BetaFeedbackListParameters
{
  public string? Status { get; init; }
  public string? Category { get; init; }
  public int? Page { get; init; }
  public int? PageSize { get; init; }
}
