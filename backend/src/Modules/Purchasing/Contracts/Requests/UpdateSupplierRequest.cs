namespace SaasCommerce.Modules.Purchasing.Contracts.Requests;

public sealed record UpdateSupplierRequest(
  string Name,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address,
  bool IsActive);
