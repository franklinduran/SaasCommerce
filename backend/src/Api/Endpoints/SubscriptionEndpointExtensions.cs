using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Billing.Application.Subscriptions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Plans;
using SaasCommerce.Modules.Billing.Contracts.Requests;

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
      .WithTags(SubscriptionPlansTag);

    // GET /api/subscription-plans/{id} - Get specific plan details
    app.MapGet(
      "/api/subscription-plans/{id:guid}",
      async (
        Guid id,
        GetSubscriptionPlanByIdQueryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetSubscriptionPlanByIdQuery(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .WithName("GetSubscriptionPlanById")
      .WithTags(SubscriptionPlansTag);

    app.MapPost(
      "/api/subscription-plans",
      async (
        CreateSubscriptionPlanRequest request,
        CreateSubscriptionPlanCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateSubscriptionPlanCommand(
            request.Name,
            request.Code,
            request.Description,
            request.MonthlyPrice,
            request.MaxBranches,
            request.MaxUsers,
            request.MaxProducts,
            request.MaxSalesPerMonth,
            request.AllowInventoryTransfers,
            request.AllowAdvancedReports,
            request.AllowAuditLogs),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization()
      .WithName("CreateSubscriptionPlan")
      .WithTags(SubscriptionPlansTag);

    app.MapPut(
      "/api/subscription-plans/{id:guid}",
      async (
        Guid id,
        UpdateSubscriptionPlanRequest request,
        UpdateSubscriptionPlanCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateSubscriptionPlanCommand(
            id,
            request.Name,
            request.Code,
            request.Description,
            request.MonthlyPrice,
            request.MaxBranches,
            request.MaxUsers,
            request.MaxProducts,
            request.MaxSalesPerMonth,
            request.AllowInventoryTransfers,
            request.AllowAdvancedReports,
            request.AllowAuditLogs),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("UpdateSubscriptionPlan")
      .WithTags(SubscriptionPlansTag);

    app.MapPost(
      "/api/subscription-plans/{id:guid}/activate",
      async (
        Guid id,
        ActivateSubscriptionPlanCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ActivateSubscriptionPlanCommand(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("ActivateSubscriptionPlan")
      .WithTags(SubscriptionPlansTag);

    app.MapPost(
      "/api/subscription-plans/{id:guid}/deactivate",
      async (
        Guid id,
        DeactivateSubscriptionPlanCommandHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new DeactivateSubscriptionPlanCommand(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("DeactivateSubscriptionPlan")
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
      .WithTags(SubscriptionTag);

    // GET /api/subscription/usage - Get subscription usage limits
    app.MapGet(
      "/api/subscription/usage",
      async (
        GetSubscriptionUsageQueryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetSubscriptionUsageQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("GetSubscriptionUsage")
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
          new ChangeBusinessPlanCommand(request.PlanId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization()
      .WithName("ChangeSubscriptionPlan")
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
      .WithTags(SubscriptionTag);
  }
}
