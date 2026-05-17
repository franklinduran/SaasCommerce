namespace SaasCommerce.Modules.Customers.Contracts.Responses;

public sealed record RegisterCustomerPaymentResponse(
  Guid CustomerId,
  Guid PaymentId,
  decimal Amount,
  decimal NewBalance);
