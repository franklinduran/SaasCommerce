using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

public sealed class GetSubscriptionUsageQueryHandler(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ISubscriptionUsageReader usageReader,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result<SubscriptionUsageResponse>> Handle(
    GetSubscriptionUsageQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid businessId || businessId == Guid.Empty)
    {
      return Result.Failure<SubscriptionUsageResponse>(SubscriptionErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var subscription = await subscriptionRepository.GetCurrentByBusinessIdAsync(
      tenantId,
      cancellationToken);

    if (subscription is null)
    {
      return Result.Failure<SubscriptionUsageResponse>(SubscriptionErrors.SubscriptionNotFound);
    }

    var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure<SubscriptionUsageResponse>(SubscriptionErrors.PlanNotFound);
    }

    var now = clock.UtcNow;
    var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
    var monthEnd = monthStart.AddMonths(1);

    var usage = new SubscriptionUsageSnapshot(
      await usageReader.CountActiveBranchesAsync(tenantId, cancellationToken),
      await usageReader.CountActiveUsersAsync(tenantId, cancellationToken),
      await usageReader.CountActiveProductsAsync(tenantId, cancellationToken),
      await usageReader.CountMonthlySalesAsync(tenantId, monthStart, monthEnd, cancellationToken));

    var response = new SubscriptionUsageResponse(
      subscription.Id,
      plan.Name,
      subscription.Status.ToString(),
      new BranchUsage(usage.Branches, plan.MaxBranches, usage.Branches >= plan.MaxBranches),
      new UserUsage(usage.Users, plan.MaxUsers, usage.Users >= plan.MaxUsers),
      new ProductUsage(usage.Products, plan.MaxProducts, usage.Products >= plan.MaxProducts),
      new SalesUsage(usage.MonthlySales, plan.MaxSalesPerMonth, usage.MonthlySales >= plan.MaxSalesPerMonth),
      GetFeatureStatuses(plan.Features),
      subscription.TrialEndsAt,
      subscription.CurrentPeriodEnd);

    return Result.Success(response);
  }

  private static IReadOnlyList<FeatureStatus> GetFeatureStatuses(SubscriptionFeature features)
    =>
    [
      new("Ventas", features.HasFlag(SubscriptionFeature.Sales)),
      new("Productos", features.HasFlag(SubscriptionFeature.Products)),
      new("Sucursales", features.HasFlag(SubscriptionFeature.Branches)),
      new("Usuarios", features.HasFlag(SubscriptionFeature.Users)),
      new("Compras", features.HasFlag(SubscriptionFeature.Purchases)),
      new("Transferencias de inventario", features.HasFlag(SubscriptionFeature.InventoryTransfers)),
      new("Facturas", features.HasFlag(SubscriptionFeature.Invoices)),
      new("Pagos", features.HasFlag(SubscriptionFeature.Payments)),
      new("Reportes avanzados", features.HasFlag(SubscriptionFeature.Reports)),
      new("Auditoria", features.HasFlag(SubscriptionFeature.AuditLogs))
    ];
}
