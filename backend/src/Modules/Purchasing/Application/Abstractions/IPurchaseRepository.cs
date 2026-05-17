using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Abstractions;

public interface IPurchaseRepository
{
  Task<Purchase?> GetAsync(
    BusinessId businessId,
    Guid purchaseId,
    CancellationToken cancellationToken = default);

  Task AddAsync(Purchase purchase, CancellationToken cancellationToken = default);

  Task<int> CountAsync(
    BusinessId businessId,
    PurchaseSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<decimal> SumTotalAsync(
    BusinessId businessId,
    PurchaseSearchCriteria criteria,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Purchase>> ListAsync(
    BusinessId businessId,
    PurchaseSearchCriteria criteria,
    CancellationToken cancellationToken = default);
}

public sealed record PurchaseSearchCriteria(
  Guid? SupplierId,
  Guid? BranchId,
  string? Status,
  string? Query,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize,
  PurchaseSortOption SortBy,
  PurchaseSortDirection SortDirection);

public enum PurchaseSortOption
{
  PurchaseDate,
  Total,
  CreatedAt
}

public enum PurchaseSortDirection
{
  Asc,
  Desc
}
