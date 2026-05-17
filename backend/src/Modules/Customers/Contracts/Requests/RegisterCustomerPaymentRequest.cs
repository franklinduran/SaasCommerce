namespace SaasCommerce.Modules.Customers.Contracts.Requests;

public sealed record RegisterCustomerPaymentRequest(
  decimal Amount,
  string? Note);
