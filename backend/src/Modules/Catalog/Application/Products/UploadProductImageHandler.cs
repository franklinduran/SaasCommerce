using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed record UploadProductImageCommand(Guid ProductId, string ImageUrl);

public sealed class UploadProductImageHandler(
  ICatalogProductRepository products,
  IUnitOfWork unitOfWork,
  ICurrentUserService currentUser,
  IClock clock)
{
  public Task<Result<ProductResponse>> Handle(
    UploadProductImageCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<ProductResponse>> HandleCoreAsync(
    UploadProductImageCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.UserContextRequired);
    }

    var product = await products.GetByIdAsync(
      command.ProductId,
      new BusinessId(businessId),
      cancellationToken);

    if (product is null)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.ProductNotFound);
    }

    product.SetImageUrl(command.ImageUrl, clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(ProductResponseMapper.ToResponse(product));
  }
}
