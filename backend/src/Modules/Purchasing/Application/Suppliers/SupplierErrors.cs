using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public static class SupplierErrors
{
  public static readonly DomainError InvalidSupplier =
    new("suppliers.invalid_supplier", "The supplier request is invalid.");

  public static readonly DomainError SupplierNotFound =
    new("suppliers.supplier_not_found", "The supplier was not found.");

  public static readonly DomainError UserContextRequired =
    new("suppliers.user_context_required", "The authenticated user does not have a valid tenant context.");
}
