namespace SaasCommerce.Modules.Customers.Contracts.Requests;

public sealed record UpdateCustomerRequest(
  string FirstName,
  string LastName,
  string? Phone,
  string? Email,
  bool IsActive = true,
  string? Cedula = null);
