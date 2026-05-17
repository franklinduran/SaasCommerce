using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed class ListCustomersUseCase(
  ICustomerRepository customers,
  ICurrentUserService currentUser) : IListCustomersUseCase
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<CustomerListResponse>> ExecuteAsync(
    ListCustomersQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return ExecuteCoreAsync(query, cancellationToken);
  }

  private async Task<Result<CustomerListResponse>> ExecuteCoreAsync(
    ListCustomersQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CustomerListResponse>(CustomerErrors.UserContextRequired);
    }

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize))
    {
      return Result.Failure<CustomerListResponse>(CustomerErrors.InvalidCustomer);
    }

    if (!TryParseSort(query.SortBy, query.SortDirection, out var sortBy, out var sortDirection))
    {
      return Result.Failure<CustomerListResponse>(CustomerErrors.InvalidCustomer);
    }

    var tenantId = new BusinessId(businessId);
    var criteria = new CustomerSearchCriteria(
      query.Query,
      query.IsActive,
      query.Page,
      query.PageSize,
      sortBy,
      sortDirection);
    var totalItems = await customers.CountAsync(tenantId, criteria, cancellationToken);
    var items = await customers.ListAsync(tenantId, criteria, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

    return Result.Success(new CustomerListResponse(
      items.Select(CustomerResponseMapper.ToResponse).ToArray(),
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
    out CustomerSortOption sortBy,
    out CustomerSortDirection sortDirection)
  {
    sortBy = CustomerSortOption.FullName;
    sortDirection = CustomerSortDirection.Asc;

    if (!string.IsNullOrWhiteSpace(sortByValue) &&
        !Enum.TryParse(sortByValue, true, out sortBy))
    {
      return false;
    }

    return string.IsNullOrWhiteSpace(sortDirectionValue) ||
      Enum.TryParse(sortDirectionValue, true, out sortDirection);
  }
}
