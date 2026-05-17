namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public sealed record CreateSupplierCommand(
  string Name,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address);
