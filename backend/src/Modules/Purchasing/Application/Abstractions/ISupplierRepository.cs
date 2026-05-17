using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Abstractions;

public interface ISupplierRepository
{
  Task<Supplier?> GetAsync(
    BusinessId businessId,
    Guid supplierId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    SupplierSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Supplier>> ListAsync(
    BusinessId businessId,
    SupplierSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyDictionary<Guid, Supplier>> ListByIdsAsync(
    BusinessId businessId,
    IReadOnlyCollection<Guid> supplierIds,
    CancellationToken cancellationToken = default);
}

public sealed record SupplierSearchCriteria(
  string? Query,
  bool? IsActive,
  int Page,
  int PageSize,
  SupplierSortOption SortBy,
  SupplierSortDirection SortDirection);

public enum SupplierSortOption
{
  Name,
  CreatedAt
}

public enum SupplierSortDirection
{
  Asc,
  Desc
}
