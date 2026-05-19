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

    // GET /api/users/{id:guid}
    app.MapGet(
      "/api/users/{id:guid}",
      async (
        Guid id,
        GetUserByIdHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetUserByIdQuery(id),
          cancellationToken);

        return result.IsSuccess
          ? Results.Ok(ApiResponse.Success(result.Value, correlationIdProvider.CorrelationId))
          : Results.Json(
            ApiResponse.Failure<object?>(
              new ApiError(ApiHelpers.ToPublicErrorCode(result.Error.Code), result.Error.Message),
              correlationIdProvider.CorrelationId),
            statusCode: ApiHelpers.ToFailureStatusCode(ApiHelpers.ToPublicErrorCode(result.Error.Code)));
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersView}")
      .WithTags(UsersTag);

    // POST /api/users
    app.MapPost(
      "/api/users",
      async (
        CreateUserRequest request,
        CreateUserHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateUserCommand(
            request.FullName,
            request.Email,
            request.Password,
            request.Role,
            request.DefaultBranchId),
          cancellationToken);

        return result.IsSuccess
          ? Results.Created($"/api/users/{result.Value}", ApiResponse.Success(new { userId = result.Value }, correlationIdProvider.CorrelationId))
          : Results.Json(
            ApiResponse.Failure<object?>(
              new ApiError(ApiHelpers.ToPublicErrorCode(result.Error.Code), result.Error.Message),
              correlationIdProvider.CorrelationId),
            statusCode: ApiHelpers.ToFailureStatusCode(ApiHelpers.ToPublicErrorCode(result.Error.Code)));
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersCreate}")
      .WithTags(UsersTag);

    // PUT /api/users/{id:guid}
    app.MapPut(
      "/api/users/{id:guid}",
      async (
        Guid id,
        UpdateUserRequest request,
        UpdateUserHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateUserCommand(
            id,
            request.FullName,
            request.Phone,
            request.DefaultBranchId),
          cancellationToken);

        return result.IsSuccess
          ? Results.Ok(ApiResponse.Success<object?>(null, correlationIdProvider.CorrelationId))
          : Results.Json(
            ApiResponse.Failure<object?>(
              new ApiError(ApiHelpers.ToPublicErrorCode(result.Error.Code), result.Error.Message),
              correlationIdProvider.CorrelationId),
            statusCode: ApiHelpers.ToFailureStatusCode(ApiHelpers.ToPublicErrorCode(result.Error.Code)));
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersUpdate}")
      .WithTags(UsersTag);

    // POST /api/users/{id:guid}/activate
    app.MapPost(
      "/api/users/{id:guid}/activate",
      async (
        Guid id,
        ActivateUserHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new ActivateUserCommand(id),
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

    // POST /api/users/{id:guid}/reset-password
    app.MapPost(
      "/api/users/{id:guid}/reset-password",
      async (
        Guid id,
        ResetUserPasswordHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new ResetUserPasswordCommand(id),
          cancellationToken);

        return result.IsSuccess
          ? Results.Ok(ApiResponse.Success(result.Value, correlationIdProvider.CorrelationId))
          : Results.Json(
            ApiResponse.Failure<object?>(
              new ApiError(ApiHelpers.ToPublicErrorCode(result.Error.Code), result.Error.Message),
              correlationIdProvider.CorrelationId),
            statusCode: ApiHelpers.ToFailureStatusCode(ApiHelpers.ToPublicErrorCode(result.Error.Code)));
      })
      .RequireAuthorization($"Permission:{SystemPermissions.UsersResetPassword}")
      .WithTags(UsersTag);

    return app;
  }
}
