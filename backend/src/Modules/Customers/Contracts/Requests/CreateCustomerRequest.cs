namespace SaasCommerce.Modules.Customers.Contracts.Requests;

public sealed record CreateCustomerRequest(
  string FullName,
  string? Phone,
  string? Email);
