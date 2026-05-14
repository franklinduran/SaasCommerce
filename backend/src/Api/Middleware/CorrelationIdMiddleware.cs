using Serilog.Context;

namespace SaasCommerce.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
  public const string HeaderName = "X-Correlation-Id";

  public async Task InvokeAsync(HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var correlationId = GetOrCreateCorrelationId(context);
    context.TraceIdentifier = correlationId;
    context.Items[HeaderName] = correlationId;

    context.Response.OnStarting(() =>
    {
      context.Response.Headers[HeaderName] = correlationId;
      return Task.CompletedTask;
    });

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
      await next(context);
    }
  }

  private static string GetOrCreateCorrelationId(HttpContext context)
  {
    if (context.Request.Headers.TryGetValue(HeaderName, out var values))
    {
      var correlationId = values.FirstOrDefault();

      if (!string.IsNullOrWhiteSpace(correlationId))
      {
        return correlationId.Trim();
      }
    }

    return Guid.NewGuid().ToString("D");
  }
}
