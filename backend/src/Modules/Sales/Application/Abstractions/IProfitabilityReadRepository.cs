using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface IProfitabilityReadRepository
{
  Task<ProfitabilitySummaryResponse> GetSummaryAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<ProductProfitabilityResponse>> GetProductProfitabilityAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    Guid? branchId,
    Guid? categoryId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<BranchProfitabilityResponse>> GetBranchProfitabilityAsync(
    BusinessId businessId,
    DateTimeOffset dateFrom,
    DateTimeOffset dateTo,
    CancellationToken cancellationToken = default);
}
