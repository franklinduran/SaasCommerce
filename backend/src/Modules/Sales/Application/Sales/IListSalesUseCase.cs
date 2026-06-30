using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IListSalesUseCase
{
  Task<Result<SaleListResponse>> ExecuteAsync(
    ListSalesQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record ListSalesQuery(
  Guid? BranchId,
  string? Status,
  string? PaymentMethod,
  string? Query,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
