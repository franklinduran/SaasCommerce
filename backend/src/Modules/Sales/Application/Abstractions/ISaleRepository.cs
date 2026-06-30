using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ISaleRepository
{
  Task<Sale?> GetAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Sale sale, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Sale>> ListAsync(
    BusinessId businessId,
    SaleSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record SaleSearchCriteria(
  Guid? BranchId,
  string? Status,
  string? PaymentMethod,
  string? Query,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize,
  SaleSortOption SortBy,
  SaleSortDirection SortDirection);

public enum SaleSortOption
{
  CreatedAt,
  Total
}

public enum SaleSortDirection
{
  Asc,
  Desc
}
