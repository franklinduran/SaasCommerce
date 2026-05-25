using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Application.Onboarding;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Api.Endpoints;

internal static class OnboardingEndpointExtensions
{
  private const string OnboardingTag = "Onboarding";

  internal static WebApplication MapOnboardingEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/onboarding/status",
      async (
        GetOnboardingStatusHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.OnboardingView}")
      .WithTags(OnboardingTag);

    app.MapPost(
      "/api/onboarding/steps/{step}/complete",
      async (
        string step,
        CompleteOnboardingStepHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(step, cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.OnboardingManage}")
      .WithTags(OnboardingTag);

    return app;
  }
}
