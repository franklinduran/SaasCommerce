namespace SaasCommerce.Modules.Customers.Contracts.Responses;

public sealed record CustomerResponse(
  Guid Id,
  Guid BusinessId,
  string FullName,
  string? Phone,
  string? Email,
  bool IsActive,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt,
  DateTimeOffset? DeactivatedAt,
  decimal CurrentBalance = 0,
  decimal CreditLimit = 0,
  string CreditStatus = "Active");
