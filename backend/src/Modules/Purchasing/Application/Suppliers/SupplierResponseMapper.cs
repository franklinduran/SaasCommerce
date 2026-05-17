using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.Modules.Purchasing.Domain;

namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

internal static class SupplierResponseMapper
{
  public static SupplierResponse ToResponse(Supplier supplier)
  {
    ArgumentNullException.ThrowIfNull(supplier);

    return new SupplierResponse(
      supplier.Id,
      supplier.BusinessId.Value,
      supplier.Name,
      supplier.Rnc,
      supplier.Phone,
      supplier.Email,
      supplier.Address,
      supplier.IsActive,
      supplier.CreatedAt,
      supplier.UpdatedAt);
  }
}
