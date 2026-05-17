using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public static class SalesErrors
{
  public static readonly DomainError InvalidSale =
    new("SALES_INVALID_SALE", "The sale request is invalid.");

  public static readonly DomainError SaleNotFound =
    new("SALES_SALE_NOT_FOUND", "The sale was not found.");

  public static readonly DomainError InvalidSaleState =
    new("SALES_INVALID_STATE", "The sale cannot move to the requested state.");
}
