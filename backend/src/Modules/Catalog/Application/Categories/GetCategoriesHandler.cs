using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Categories;

public sealed class GetCategoriesHandler(
  ICatalogCategoryRepository categories,
  ICurrentUserService currentUser)
{
  public async Task<Result<IReadOnlyCollection<CategoryResponse>>> Handle(
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<IReadOnlyCollection<CategoryResponse>>(CatalogErrors.UserContextRequired);
    }

    var items = await categories.ListAsync(new BusinessId(businessId), cancellationToken);

    return Result.Success<IReadOnlyCollection<CategoryResponse>>(
      items.Select(CategoryResponseMapper.ToResponse).ToArray());
  }
}
