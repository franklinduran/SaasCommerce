using SaasCommerce.Api;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Identity.Application.Users;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class UsersEndpointExtensions
{
  private const string UsersTag = "Users";

  internal static WebApplication MapUsersEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/users",
      async (
        GetUsersHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetUsersQuery(), cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersView}")
      .WithTags(UsersTag);

    app.MapPut(
      "/api/users/{id:guid}/role",
      async (
        Guid id,
        UpdateUserRoleRequest request,
        UpdateUserRoleHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateUserRoleCommand(id, request.Role),
          cancellationToken);

        return result.IsSuccess
          ? Results.Ok(ApiResponse.Success<object?>(null, correlationIdProvider.CorrelationId))
          : Results.Json(
            ApiResponse.Failure<object?>(
              new ApiError(ApiHelpers.ToPublicErrorCode(result.Error.Code), result.Error.Message),
              correlationIdProvider.CorrelationId),
            statusCode: ApiHelpers.ToFailureStatusCode(ApiHelpers.ToPublicErrorCode(result.Error.Code)));
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersUpdateRole}")
      .WithTags(UsersTag);

    app.MapPut(
      "/api/users/{id:guid}/disable",
      async (
        Guid id,
        DisableUserHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new DisableUserCommand(id),
          cancellationToken);

        return result.IsSuccess
          ? Results.Ok(ApiResponse.Success<object?>(null, correlationIdProvider.CorrelationId))
          : Results.Json(
            ApiResponse.Failure<object?>(
              new ApiError(ApiHelpers.ToPublicErrorCode(result.Error.Code), result.Error.Message),
              correlationIdProvider.CorrelationId),
            statusCode: ApiHelpers.ToFailureStatusCode(ApiHelpers.ToPublicErrorCode(result.Error.Code)));
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersDisable}")
      .WithTags(UsersTag);

    return app;
  }
}
