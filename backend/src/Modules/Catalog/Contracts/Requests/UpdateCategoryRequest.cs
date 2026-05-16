namespace SaasCommerce.Modules.Catalog.Contracts.Requests;

public sealed record UpdateCategoryRequest(
  string Name,
  string? Description,
  bool IsActive);
