namespace SaasCommerce.Modules.Catalog.Contracts.Requests;

public sealed record CreateCategoryRequest(
  string Name,
  string? Description);
