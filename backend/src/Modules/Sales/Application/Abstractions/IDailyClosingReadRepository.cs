using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface IDailyClosingReadRepository
{
  Task<(IReadOnlyCollection<DailyClosingListItemResponse> Items, int TotalCount)> GetListAsync(
    BusinessId businessId,
    Guid? branchId,
    DateOnly? dateFrom,
    DateOnly? dateTo,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

  Task<DailyClosingDetailResponse?> GetDetailAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}
