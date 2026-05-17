namespace SaasCommerce.Modules.Customers.Contracts.Requests;

public sealed record UpdateCustomerRequest(
  string FullName,
  string? Phone,
  string? Email,
  bool IsActive = true);
