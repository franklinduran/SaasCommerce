using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Profitability;

public static class ProfitabilityErrors
{
  public static readonly DomainError UserContextRequired =
    new("profitability.user_context_required",
      "The authenticated user does not have a valid tenant context.");
}
