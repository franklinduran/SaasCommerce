using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Abstractions;

public sealed record SubscriptionUsageSnapshot(
  int Branches,
  int Users,
  int Products,
  int MonthlySales);

public interface ISubscriptionUsageReader
{
  Task<int> CountActiveBranchesAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<int> CountActiveUsersAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<int> CountActiveProductsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<int> CountMonthlySalesAsync(
    BusinessId businessId,
    DateTimeOffset monthStart,
    DateTimeOffset monthEnd,
    CancellationToken cancellationToken = default);
}
