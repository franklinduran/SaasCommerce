using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ICashRegisterCalculator
{
  Task<CashRegisterTotals> CalculateAsync(
    BusinessId businessId,
    BranchId branchId,
    DateTimeOffset openedAt,
    DateTimeOffset closedAt,
    CancellationToken ct);
}
