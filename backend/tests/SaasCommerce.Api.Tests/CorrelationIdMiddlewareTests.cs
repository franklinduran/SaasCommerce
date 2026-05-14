using SaasCommerce.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace SaasCommerce.Api.Tests;

public sealed class CorrelationIdMiddlewareTests
{
  [Fact]
  public async Task InvokeAsyncShouldReuseIncomingCorrelationId()
  {
    const string correlationId = "incoming-correlation-id";
    var context = CreateHttpContext();
    context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
    var middleware = new CorrelationIdMiddleware(async httpContext =>
    {
      httpContext.TraceIdentifier.Should().Be(correlationId);
      httpContext.Items[CorrelationIdMiddleware.HeaderName].Should().Be(correlationId);

      await httpContext.Response.WriteAsync("ok");
    });

    await middleware.InvokeAsync(context);

    context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().Should().Be(correlationId);
  }

  [Fact]
  public async Task InvokeAsyncShouldGenerateCorrelationIdWhenHeaderIsMissing()
  {
    var context = CreateHttpContext();
    var middleware = new CorrelationIdMiddleware(async httpContext =>
    {
      httpContext.TraceIdentifier.Should().NotBeNullOrWhiteSpace();
      Guid.TryParse(httpContext.TraceIdentifier, out _).Should().BeTrue();

      await httpContext.Response.WriteAsync("ok");
    });

    await middleware.InvokeAsync(context);

    var responseCorrelationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
    responseCorrelationId.Should().NotBeNullOrWhiteSpace();
    Guid.TryParse(responseCorrelationId, out _).Should().BeTrue();
  }

  private static DefaultHttpContext CreateHttpContext()
  {
    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();

    return context;
  }
}
