using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed class GetProductHandler(
  ICatalogProductRepository products,
  ICurrentUserService currentUser)
{
  public Task<Result<ProductResponse>> Handle(
    GetProductQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<ProductResponse>> HandleCoreAsync(
    GetProductQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.UserContextRequired);
    }

    var product = await products.GetByIdAsync(
      query.ProductId,
      new BusinessId(businessId),
      cancellationToken);

    return product is null
      ? Result.Failure<ProductResponse>(CatalogErrors.ProductNotFound)
      : Result.Success(ProductResponseMapper.ToResponse(product));
  }
}
