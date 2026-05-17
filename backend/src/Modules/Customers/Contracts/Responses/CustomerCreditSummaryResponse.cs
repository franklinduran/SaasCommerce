namespace SaasCommerce.Modules.Customers.Contracts.Responses;

public sealed record CustomerCreditSummaryResponse(
  Guid BusinessId,
  Guid CustomerId,
  string CustomerName,
  decimal CreditLimit,
  decimal CurrentBalance,
  string Status,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);
