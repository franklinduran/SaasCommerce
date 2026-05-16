using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed class GetProductsHandler(
  ICatalogProductRepository products,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<ProductListResponse>> Handle(
    GetProductsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<ProductListResponse>> HandleCoreAsync(
    GetProductsQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProductListResponse>(CatalogErrors.UserContextRequired);
    }

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize))
    {
      return Result.Failure<ProductListResponse>(CatalogErrors.InvalidProduct);
    }

    var page = query.Page;
    var pageSize = query.PageSize;
    ProductType? productType = null;

    if (!string.IsNullOrWhiteSpace(query.ProductType))
    {
      if (!Enum.TryParse<ProductType>(query.ProductType, true, out var parsedType))
      {
        return Result.Failure<ProductListResponse>(CatalogErrors.InvalidProduct);
      }

      productType = parsedType;
    }

    if (!TryParseSort(query.SortBy, query.SortDirection, out var sortBy, out var sortDirection))
    {
      return Result.Failure<ProductListResponse>(CatalogErrors.InvalidProduct);
    }

    var tenantId = new BusinessId(businessId);
    var criteria = new ProductSearchCriteria(
      query.Query,
      productType,
      query.CategoryId,
      query.IsActive,
      page,
      pageSize,
      sortBy,
      sortDirection);
    var totalItems = await products.CountAsync(
      tenantId,
      criteria,
      cancellationToken);
    var items = await products.ListAsync(
      tenantId,
      criteria,
      cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return Result.Success(new ProductListResponse(
      items.Select(ProductResponseMapper.ToResponse).ToArray(),
      page,
      pageSize,
      totalItems,
      totalPages,
      page > 1,
      totalPages > 0 && page < totalPages));
  }

  private static bool TryParseSort(
    string? sortByValue,
    string? sortDirectionValue,
    out ProductSortOption sortBy,
    out SortDirection sortDirection)
  {
    sortBy = ProductSortOption.Name;
    sortDirection = SortDirection.Asc;

    if (!string.IsNullOrWhiteSpace(sortByValue) &&
        !Enum.TryParse(sortByValue, true, out sortBy))
    {
      return false;
    }

    return string.IsNullOrWhiteSpace(sortDirectionValue) ||
      Enum.TryParse(sortDirectionValue, true, out sortDirection);
  }
}
