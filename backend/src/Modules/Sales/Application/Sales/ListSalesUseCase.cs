using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class ListSalesUseCase(
  ISaleRepository sales,
  ICurrentUserService currentUser) : IListSalesUseCase
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<SaleListResponse>> ExecuteAsync(
    ListSalesQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return ExecuteCoreAsync(query, cancellationToken);
  }

  private async Task<Result<SaleListResponse>> ExecuteCoreAsync(
    ListSalesQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<SaleListResponse>(SalesErrors.UserContextRequired);
    }

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize))
    {
      return Result.Failure<SaleListResponse>(SalesErrors.InvalidSale);
    }

    if (!string.IsNullOrWhiteSpace(query.Status) &&
        !Enum.TryParse<SaleStatus>(query.Status, true, out _))
    {
      return Result.Failure<SaleListResponse>(SalesErrors.InvalidSale);
    }

    if (!TryParseSort(query.SortBy, query.SortDirection, out var sortBy, out var sortDirection))
    {
      return Result.Failure<SaleListResponse>(SalesErrors.InvalidSale);
    }

    var tenantId = new BusinessId(businessId);
    var criteria = new SaleSearchCriteria(
      query.BranchId,
      query.Status,
      query.DateFrom,
      query.DateTo,
      query.Page,
      query.PageSize,
      sortBy,
      sortDirection);
    var totalItems = await sales.CountAsync(tenantId, criteria, cancellationToken);
    var items = await sales.ListAsync(tenantId, criteria, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

    return Result.Success(new SaleListResponse(
      items.Select(SaleResponseMapper.ToResponse).ToArray(),
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
    out SaleSortOption sortBy,
    out SaleSortDirection sortDirection)
  {
    sortBy = SaleSortOption.CreatedAt;
    sortDirection = SaleSortDirection.Desc;

    if (!string.IsNullOrWhiteSpace(sortByValue) &&
        !Enum.TryParse(sortByValue, true, out sortBy))
    {
      return false;
    }

    return string.IsNullOrWhiteSpace(sortDirectionValue) ||
      Enum.TryParse(sortDirectionValue, true, out sortDirection);
  }
}
