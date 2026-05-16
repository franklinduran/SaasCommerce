using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed class ActivateProductHandler(
  ICatalogProductRepository products,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<ProductResponse>> Handle(
    ActivateProductCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<ProductResponse>> HandleCoreAsync(
    ActivateProductCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId || command.ProductId == Guid.Empty)
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

    product.Activate(clock.UtcNow);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(ProductResponseMapper.ToResponse(product));
  }
}
