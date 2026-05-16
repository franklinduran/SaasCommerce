using Microsoft.AspNetCore.Http;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Observability;

public sealed class CorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
  : ICorrelationIdProvider
{
  public const string HeaderName = "X-Correlation-Id";

  public string CorrelationId
  {
    get
    {
      var httpContext = httpContextAccessor.HttpContext;

      if (httpContext is null)
      {
        return Guid.NewGuid().ToString("D");
      }

      if (httpContext.Items.TryGetValue(HeaderName, out var value) &&
          value is string correlationId &&
          !string.IsNullOrWhiteSpace(correlationId))
      {
        return correlationId;
      }

      return httpContext.TraceIdentifier;
    }
  }
}
