using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Reporting.Application;

public static class ReportingErrors
{
  public static readonly DomainError UserContextRequired =
    new("reporting.user_context_required", "User business context is required.");
}
