namespace SaasCommerce.Api.Middleware;

/// <summary>
/// Adds security-related HTTP response headers to every response.
/// Should be registered early in the pipeline, after correlation ID but before routing.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
  public Task InvokeAsync(HttpContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var headers = context.Response.Headers;

    // Prevents browsers from MIME-type sniffing a response away from the declared content-type.
    headers["X-Content-Type-Options"] = "nosniff";

    // Prevents the page from being embedded in an iframe (clickjacking protection).
    headers["X-Frame-Options"] = "DENY";

    // Controls how much referrer information is included with requests.
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Disables the browser's built-in XSS auditor (modern browsers no longer use it and
    // enabling it can introduce new vulnerabilities).
    headers["X-XSS-Protection"] = "0";

    // Explicitly opt out of FLoC/Privacy Sandbox tracking in Chromium-based browsers.
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    return next(context);
  }
}
