namespace SaasCommerce.Modules.Customers.Contracts.Responses;

public sealed record CustomerCreditMovementResponse(
  Guid Id,
  Guid BusinessId,
  Guid CustomerId,
  Guid? SaleId,
  Guid? PaymentId,
  string Type,
  decimal Amount,
  decimal PreviousBalance,
  decimal NewBalance,
  string? Note,
  DateTimeOffset CreatedAt,
  Guid? CreatedBy);
