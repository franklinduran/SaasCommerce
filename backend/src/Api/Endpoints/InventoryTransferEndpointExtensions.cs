using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Inventory.Application.Transfers;
using SaasCommerce.Modules.Inventory.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class InventoryTransferEndpointExtensions
{
  private const string InventoryTransferTag = "InventoryTransfers";

  internal static WebApplication MapInventoryTransferEndpoints(this WebApplication app)
  {
    app.MapPost(
      "/api/inventory-transfers",
      async (
        CreateInventoryTransferRequest request,
        CreateInventoryTransferHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateInventoryTransferCommand(
            request.SourceBranchId,
            request.TargetBranchId,
            request.Items
              .Select(item => new CreateInventoryTransferItemCommand(item.ProductId, item.Quantity))
              .ToArray(),
            request.Note),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.InventoryTransfer}")
      .WithTags(InventoryTransferTag);

    app.MapGet(
      "/api/inventory-transfers",
      async (
        [AsParameters] InventoryTransferListEndpointRequest request,
        GetInventoryTransfersHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetInventoryTransfersQuery(
            request.SourceBranchId,
            request.TargetBranchId,
            request.Status,
            request.Page ?? 1,
            request.PageSize ?? 10),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.InventoryTransfer}")
      .WithTags(InventoryTransferTag);

    app.MapGet(
      "/api/inventory-transfers/{id:guid}",
      async (
        Guid id,
        GetInventoryTransferByIdHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetInventoryTransferByIdQuery(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.InventoryTransfer}")
      .WithTags(InventoryTransferTag);

    app.MapPost(
      "/api/inventory-transfers/{id:guid}/cancel",
      async (
        Guid id,
        CancelInventoryTransferHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new CancelInventoryTransferCommand(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.InventoryTransfer}")
      .WithTags(InventoryTransferTag);

    return app;
  }
}
