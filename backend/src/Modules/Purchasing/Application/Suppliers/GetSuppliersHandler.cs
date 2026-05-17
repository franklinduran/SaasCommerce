using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public sealed class GetSuppliersHandler(
  ISupplierRepository suppliers,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<SupplierListResponse>> Handle(
    GetSuppliersQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<SupplierListResponse>> HandleCoreAsync(
    GetSuppliersQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<SupplierListResponse>(SupplierErrors.UserContextRequired);
    }

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize) ||
        !TryParseSort(query.SortBy, query.SortDirection, out var sortBy, out var sortDirection))
    {
      return Result.Failure<SupplierListResponse>(SupplierErrors.InvalidSupplier);
    }

    var tenantId = new BusinessId(businessId);
    var criteria = new SupplierSearchCriteria(
      query.Query,
      query.IsActive,
      query.Page,
      query.PageSize,
      sortBy,
      sortDirection);
    var totalItems = await suppliers.CountAsync(tenantId, criteria, cancellationToken);
    var items = await suppliers.ListAsync(tenantId, criteria, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

    return Result.Success(new SupplierListResponse(
      items.Select(SupplierResponseMapper.ToResponse).ToArray(),
      query.Page,
      query.PageSize,
      totalItems,
      totalPages,
      query.Page > 1,
      totalPages > 0 && query.Page < totalPages));
  }

  private static bool TryParseSort(
    string? sortByValue,
    string? sortDirectionValue,
    out SupplierSortOption sortBy,
    out SupplierSortDirection sortDirection)
  {
    sortBy = SupplierSortOption.Name;
    sortDirection = SupplierSortDirection.Asc;

    if (!string.IsNullOrWhiteSpace(sortByValue) &&
        !Enum.TryParse(sortByValue, true, out sortBy))
    {
      return false;
    }

    return string.IsNullOrWhiteSpace(sortDirectionValue) ||
      Enum.TryParse(sortDirectionValue, true, out sortDirection);
  }
}
