using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Tenancy.Application.Branches;
using SaasCommerce.Modules.Tenancy.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class BranchEndpointExtensions
{
  private const string BranchesTag = "Branches";

  internal static WebApplication MapBranchEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/branches",
      async (
        [AsParameters] BranchListEndpointRequest request,
        GetBranchesHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetBranchesQuery(request.IsActive), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BranchesView}")
      .WithTags(BranchesTag);

    app.MapGet(
      "/api/branches/{id:guid}",
      async (
        Guid id,
        GetBranchByIdHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetBranchByIdQuery(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BranchesView}")
      .WithTags(BranchesTag);

    app.MapPost(
      "/api/branches",
      async (
        CreateBranchRequest request,
        CreateBranchHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateBranchCommand(request.Name, request.Code, request.Address, request.Phone, request.IsMain),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BranchesCreate}")
      .WithTags(BranchesTag);

    app.MapPut(
      "/api/branches/{id:guid}",
      async (
        Guid id,
        UpdateBranchRequest request,
        UpdateBranchHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateBranchCommand(id, request.Name, request.Address, request.Phone),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BranchesUpdate}")
      .WithTags(BranchesTag);

    app.MapPost(
      "/api/branches/{id:guid}/activate",
      async (
        Guid id,
        ActivateBranchHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ActivateBranchCommand(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BranchesUpdate}")
      .WithTags(BranchesTag);

    app.MapPost(
      "/api/branches/{id:guid}/deactivate",
      async (
        Guid id,
        DeactivateBranchHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new DeactivateBranchCommand(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.BranchesUpdate}")
      .WithTags(BranchesTag);

    return app;
  }
}
