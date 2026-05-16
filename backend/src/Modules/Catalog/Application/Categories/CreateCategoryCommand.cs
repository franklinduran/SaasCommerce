namespace SaasCommerce.Modules.Catalog.Application.Categories;

public sealed record CreateCategoryCommand(
  string Name,
  string? Description);
