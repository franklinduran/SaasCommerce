using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Categories;

public sealed class UpdateCategoryHandler(
  ICatalogCategoryRepository categories,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<CategoryResponse>> Handle(
    UpdateCategoryCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CategoryResponse>> HandleCoreAsync(
    UpdateCategoryCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.UserContextRequired);
    }

    if (command.Id == Guid.Empty || string.IsNullOrWhiteSpace(command.Name))
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.InvalidProduct);
    }

    var tenantId = new BusinessId(businessId);
    var category = await categories.GetByIdAsync(tenantId, command.Id, cancellationToken);

    if (category is null)
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.CategoryNotFound);
    }

    var existing = await categories.GetByNameAsync(tenantId, command.Name, cancellationToken);

    if (existing is not null && existing.Id != command.Id)
    {
      return Result.Failure<CategoryResponse>(CatalogErrors.DuplicateCategory);
    }

    category.Update(command.Name, command.Description, command.IsActive, clock.UtcNow);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CategoryResponseMapper.ToResponse(category));
  }
}
