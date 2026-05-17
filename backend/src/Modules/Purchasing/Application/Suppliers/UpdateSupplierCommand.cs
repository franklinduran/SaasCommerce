namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public sealed record UpdateSupplierCommand(
  Guid SupplierId,
  string Name,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address,
  bool IsActive);
