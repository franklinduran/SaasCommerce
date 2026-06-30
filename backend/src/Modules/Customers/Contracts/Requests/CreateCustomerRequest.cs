namespace SaasCommerce.Modules.Customers.Contracts.Requests;

public sealed record CreateCustomerRequest(
  string FirstName,
  string LastName,
  string? Phone,
  string? Email,
  string? Cedula = null);
