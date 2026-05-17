using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Abstractions;

public interface ICustomerRepository
{
  Task<Customer?> GetAsync(
    BusinessId businessId,
    Guid customerId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    CustomerSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Customer>> ListAsync(
    BusinessId businessId,
    CustomerSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record CustomerSearchCriteria(
  string? Query,
  bool? IsActive,
  int Page,
  int PageSize,
  CustomerSortOption SortBy,
  CustomerSortDirection SortDirection);

public enum CustomerSortOption
{
  FullName,
  CreatedAt
}

public enum CustomerSortDirection
{
  Asc,
  Desc
}
