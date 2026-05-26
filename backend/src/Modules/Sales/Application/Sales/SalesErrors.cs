using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public static class SalesErrors
{
  public static readonly DomainError InvalidSale =
    new("sales.invalid_sale", "The sale request is invalid.");

  public static readonly DomainError SaleNotFound =
    new("sales.sale_not_found", "The sale was not found.");

  public static readonly DomainError InvalidSaleState =
    new("sales.invalid_state", "The sale cannot move to the requested state.");

  public static readonly DomainError UserContextRequired =
    new("sales.user_context_required", "The authenticated user does not have a valid tenant context.");

  public static readonly DomainError ProductNotFound =
    new("sales.product_not_found", "One or more sale products were not found.");

  public static readonly DomainError CustomerNotFound =
    new("sales.customer_not_found", "The sale customer was not found.");

  public static readonly DomainError InvalidSaleReturn =
    new("sales.invalid_return", "The sale return request is invalid.");

  public static readonly DomainError SaleReturnNotFound =
    new("sales.return_not_found", "The sale return was not found.");

  public static readonly DomainError SaleReturnAlreadyProcessed =
    new("sales.return_already_processed", "The sale return was already processed.");
}
