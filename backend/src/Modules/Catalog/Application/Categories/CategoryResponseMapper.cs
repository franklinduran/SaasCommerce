using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;

namespace SaasCommerce.Modules.Catalog.Application.Categories;

internal static class CategoryResponseMapper
{
  public static CategoryResponse ToResponse(Category category)
  {
    ArgumentNullException.ThrowIfNull(category);

    return new CategoryResponse(
      category.Id,
      category.BusinessId.Value,
      category.Name,
      category.Description,
      category.IsActive);
  }
}
