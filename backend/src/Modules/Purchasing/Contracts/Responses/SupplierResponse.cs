namespace SaasCommerce.Modules.Purchasing.Contracts.Responses;

public sealed record SupplierResponse(
  Guid Id,
  Guid BusinessId,
  string Name,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address,
  bool IsActive,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt);
