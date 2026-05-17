namespace SaasCommerce.Modules.Purchasing.Contracts.Requests;

public sealed record CreateSupplierRequest(
  string Name,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address);
