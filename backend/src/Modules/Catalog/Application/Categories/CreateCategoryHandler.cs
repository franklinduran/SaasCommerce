using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Categories;

public sealed class CreateCategoryHandler(
  ICatalogCategoryRepository categories,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<CategoryResponse>> Handle(
    CreateCategoryCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CategoryResponse>> HandleCoreAsync(
    CreateCategoryCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.UserContextRequired);
    }

    if (string.IsNullOrWhiteSpace(command.Name))
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.InvalidProduct);
    }

    var tenantId = new BusinessId(businessId);
    var existing = await categories.GetByNameAsync(tenantId, command.Name, cancellationToken);

    if (existing is not null)
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.DuplicateCategory);
    }

    var category = new Category(Guid.NewGuid(), tenantId, command.Name, command.Description, clock.UtcNow);

    await categories.AddAsync(category, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CategoryResponseMapper.ToResponse(category));
  }
}
