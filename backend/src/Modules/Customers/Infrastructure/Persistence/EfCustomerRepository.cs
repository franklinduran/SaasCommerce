using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Infrastructure.Persistence;

public sealed class EfCustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
  public Task<Customer?> GetAsync(
    BusinessId businessId,
    Guid customerId,
    CancellationToken cancellationToken = default)
    => Customers(businessId)
      .SingleOrDefaultAsync(customer => customer.Id == customerId, cancellationToken);

  public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(customer);

    return dbContext.Set<Customer>().AddAsync(customer, cancellationToken).AsTask();
  }

  public Task<int> CountAsync(
    BusinessId businessId,
    CustomerSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => ApplyFilters(Customers(businessId).AsNoTracking(), criteria)
      .CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Customer>> ListAsync(
    BusinessId businessId,
    CustomerSearchCriteria criteria,
    CancellationToken cancellationToken = default)
    => await ApplySorting(ApplyFilters(Customers(businessId).AsNoTracking(), criteria), criteria)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .ToArrayAsync(cancellationToken);

  public async Task<IReadOnlyCollection<Customer>> ExportAllAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
    => await Customers(businessId)
      .AsNoTracking()
      .OrderBy(customer => customer.SearchName)
      .Take(10_000)
      .ToArrayAsync(cancellationToken);

  private IQueryable<Customer> Customers(BusinessId businessId)
    => dbContext.Set<Customer>()
      .Where(customer => customer.BusinessId == businessId);

  private static IQueryable<Customer> ApplyFilters(
    IQueryable<Customer> query,
    CustomerSearchCriteria criteria)
  {
    if (!string.IsNullOrWhiteSpace(criteria.Query))
    {
      var term = criteria.Query.Trim();
      var normalizedTerm = term.ToUpperInvariant();
      query = query.Where(customer =>
        customer.SearchName.Contains(normalizedTerm) ||
        customer.Phone != null && customer.Phone.Contains(term) ||
        customer.Email != null && customer.Email.Contains(term));
    }

    if (criteria.IsActive.HasValue)
    {
      query = query.Where(customer => customer.IsActive == criteria.IsActive.Value);
    }

    return query;
  }

  private static IOrderedQueryable<Customer> ApplySorting(
    IQueryable<Customer> query,
    CustomerSearchCriteria criteria)
    => (criteria.SortBy, criteria.SortDirection) switch
    {
      (CustomerSortOption.CreatedAt, CustomerSortDirection.Desc) =>
        query.OrderByDescending(customer => customer.CreatedAt),
      (CustomerSortOption.CreatedAt, _) =>
        query.OrderBy(customer => customer.CreatedAt),
      (CustomerSortOption.FullName, CustomerSortDirection.Desc) =>
        query.OrderByDescending(customer => customer.SearchName),
      _ => query.OrderBy(customer => customer.SearchName)
    };
}
