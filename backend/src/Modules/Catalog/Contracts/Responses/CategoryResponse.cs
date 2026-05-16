namespace SaasCommerce.Modules.Catalog.Contracts.Responses;

public sealed record CategoryResponse(
  Guid Id,
  Guid BusinessId,
  string Name,
  string? Description,
  bool IsActive);
