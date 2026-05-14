using SaasCommerce.Contracts.Common;

namespace SaasCommerce.Api.Middleware;

public sealed class ErrorHandlingMiddleware(
  RequestDelegate next,
  ILogger<ErrorHandlingMiddleware> logger)
{
  private const string CorrelationIdHeaderName = "X-Correlation-ID";
  private static readonly Action<ILogger, string, Exception?> LogUnhandledApiError =
    LoggerMessage.Define<string>(
      LogLevel.Error,
      new EventId(5000, nameof(LogUnhandledApiError)),
      "Unhandled API error. CorrelationId: {CorrelationId}");

  public async Task InvokeAsync(HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var correlationId = GetCorrelationId(context);
    context.Response.Headers[CorrelationIdHeaderName] = correlationId;

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
    if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var values))
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
