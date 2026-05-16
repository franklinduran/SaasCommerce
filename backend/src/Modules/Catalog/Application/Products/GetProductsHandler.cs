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

    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);
    ProductType? productType = null;

    if (!string.IsNullOrWhiteSpace(query.ProductType))
    {
      if (!Enum.TryParse<ProductType>(query.ProductType, true, out var parsedType))
      {
        return Result.Failure<ProductListResponse>(CatalogErrors.InvalidProduct);
      }

      productType = parsedType;
    }

    var tenantId = new BusinessId(businessId);
    var criteria = new ProductSearchCriteria(
      query.Query,
      productType,
      query.CategoryId,
      query.IsActive,
      page,
      pageSize);
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
      totalPages));
  }
}
