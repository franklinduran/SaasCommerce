using Serilog.Context;

namespace SaasCommerce.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
  public const string HeaderName = "X-Correlation-Id";

  public Task InvokeAsync(HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return InvokeCoreAsync(context);
  }

  private async Task InvokeCoreAsync(HttpContext context)
  {
    var correlationId = GetOrCreateCorrelationId(context);
    context.TraceIdentifier = correlationId;
    context.Items[HeaderName] = correlationId;
    context.Response.Headers[HeaderName] = correlationId;

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
