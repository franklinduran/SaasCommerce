namespace SaasCommerce.Modules.Catalog.Application.Categories;

public sealed record UpdateCategoryCommand(
  Guid Id,
  string Name,
  string? Description,
  bool IsActive);
