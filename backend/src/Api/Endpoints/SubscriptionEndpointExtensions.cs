using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Subscriptions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Plans;
using SaasCommerce.Modules.Billing.Contracts.Requests;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Api.Endpoints;

/// <summary>
/// API endpoints for subscription management.
/// Provides access to subscription plans and user subscriptions.
/// </summary>
internal static class SubscriptionEndpointExtensions
{
  private const string SubscriptionTag = "Subscriptions";
  private const string SubscriptionPlansTag = "Subscription Plans";

  /// <summary>
  /// Register all subscription-related endpoints.
  /// </summary>
  internal static WebApplication MapSubscriptionEndpoints(this WebApplication app)
  {
    MapSubscriptionPlanEndpoints(app);
    MapSubscriptionUserEndpoints(app);
    return app;
  }

  private static void MapSubscriptionPlanEndpoints(WebApplication app)
  {
    // GET /api/subscription-plans - List active subscription plans
    app.MapGet(
      "/api/subscription-plans",
      async (
        GetSubscriptionPlansQueryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetSubscriptionPlansQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .WithName("GetSubscriptionPlans")
      .WithOpenApi()
      .WithTags(SubscriptionPlansTag);

    // GET /api/subscription-plans/{id} - Get specific plan details
    app.MapGet(
      "/api/subscription-plans/{id:guid}",
      async (
        Guid id,
        ISubscriptionPlanRepository planRepository,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var plan = await planRepository.GetByIdAsync(id, cancellationToken);

        if (plan is null)
        {
          var error = new DomainError("subscription.plan_not_found", "Subscription plan not found.");
          return Results.Json(
            ApiResponse.Failure<object>(
              ApiHelpers.ToApiError(error),
              correlationIdProvider.CorrelationId),
            statusCode: StatusCodes.Status404NotFound);
        }

        var result = Modules.Billing.Application.Subscriptions.Mappers.SubscriptionPlanResponseMapper.ToResponse(plan);
        return Results.Json(
          ApiResponse.Success(result, correlationIdProvider.CorrelationId),
          statusCode: StatusCodes.Status200OK);
      })
      .WithName("GetSubscriptionPlanById")
      .WithOpenApi()
      .WithTags(SubscriptionPlansTag);
  }

  private static void MapSubscriptionUserEndpoints(WebApplication app)
  {
    // GET /api/subscription/current - Get current business subscription
    app.MapGet(
      "/api/subscription/current",
      async (
        GetCurrentBusinessSubscriptionQueryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetCurrentBusinessSubscriptionQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("GetCurrentSubscription")
      .WithOpenApi()
      .WithTags(SubscriptionTag);

    // GET /api/subscription/usage - Get subscription usage limits
    app.MapGet(
      "/api/subscription/usage",
      async (
        ISubscriptionLimitChecker limitChecker,
        ISubscriptionAccessPolicy accessPolicy,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        // Return usage information for the current business
        // This endpoint is informational and helps users understand their limits
        var response = new
        {
          message = "Subscription usage information would be returned here",
          note = "Implementation pending - fetches current usage vs limits"
        };

        return Results.Json(
          ApiResponse.Success(response, correlationIdProvider.CorrelationId),
          statusCode: StatusCodes.Status200OK);
      })
      .RequireAuthorization()
      .WithName("GetSubscriptionUsage")
      .WithOpenApi()
      .WithTags(SubscriptionTag);

    // POST /api/subscription/start-trial - Start a trial subscription (new business)
    app.MapPost(
      "/api/subscription/start-trial",
      async (
        StartTrialSubscriptionCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new StartTrialSubscriptionCommand(null), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization()
      .WithName("StartTrialSubscription")
      .WithOpenApi()
      .WithTags(SubscriptionTag);

    // POST /api/subscription/change-plan - Change subscription plan
    app.MapPost(
      "/api/subscription/change-plan",
      async (
        ChangeBusinessPlanRequest request,
        ChangeBusinessPlanCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new ChangeBusinessPlanCommand(null, request.PlanId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("ChangeSubscriptionPlan")
      .WithOpenApi()
      .WithTags(SubscriptionTag);

    // POST /api/subscription/cancel - Cancel subscription
    app.MapPost(
      "/api/subscription/cancel",
      async (
        CancelBusinessSubscriptionCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new CancelBusinessSubscriptionCommand(null), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("CancelSubscription")
      .WithOpenApi()
      .WithTags(SubscriptionTag);

    // POST /api/subscription/reactivate - Reactivate cancelled subscription
    app.MapPost(
      "/api/subscription/reactivate",
      async (
        ReactivateBusinessSubscriptionCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ReactivateBusinessSubscriptionCommand(null, null), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("ReactivateSubscription")
      .WithOpenApi()
      .WithTags(SubscriptionTag);
  }
}
