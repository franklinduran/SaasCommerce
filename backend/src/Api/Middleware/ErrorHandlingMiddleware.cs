using SaasCommerce.BuildingBlocks.Contracts.Common;

namespace SaasCommerce.Api.Middleware;

public sealed class ErrorHandlingMiddleware(
  RequestDelegate next,
  ILogger<ErrorHandlingMiddleware> logger)
{
  private static readonly Action<ILogger, string, Exception?> LogUnhandledApiError =
    LoggerMessage.Define<string>(
      LogLevel.Error,
      new EventId(5000, nameof(LogUnhandledApiError)),
      "Unhandled API error. CorrelationId: {CorrelationId}");

  public async Task InvokeAsync(HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var correlationId = GetCorrelationId(context);
    try
    {
      await next(context);
    }
    catch (Exception exception) when (!context.Response.HasStarted)
    {
      LogUnhandledApiError(logger, correlationId, exception);

      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      context.Response.ContentType = "application/json";

      var response = ApiResponse.Failure<object?>(
        new ApiError("UnhandledError", "An unexpected error occurred."),
        correlationId);

      await context.Response.WriteAsJsonAsync(response);
    }
  }

  private static string GetCorrelationId(HttpContext context)
  {
    if (context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value) &&
        value is string itemCorrelationId &&
        !string.IsNullOrWhiteSpace(itemCorrelationId))
    {
      return itemCorrelationId;
    }

    if (context.Request.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var values))
    {
      var correlationId = values.FirstOrDefault();

      if (!string.IsNullOrWhiteSpace(correlationId))
      {
        return correlationId.Trim();
      }
    }

    return context.TraceIdentifier;
  }
}
