namespace SaasCommerce.Modules.Customers.Contracts.Responses;

public sealed record CustomerResponse(
  Guid Id,
  Guid BusinessId,
  string FirstName,
  string LastName,
  string FullName,
  string? Phone,
  string? Email,
  string? Cedula,
  bool IsActive,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt,
  DateTimeOffset? DeactivatedAt,
  decimal CurrentBalance = 0,
  decimal CreditLimit = 0,
  string CreditStatus = "Active");
