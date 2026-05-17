using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public interface IListCustomersUseCase
{
  Task<Result<CustomerListResponse>> ExecuteAsync(
    ListCustomersQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record ListCustomersQuery(
  string? Query,
  bool? IsActive,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
