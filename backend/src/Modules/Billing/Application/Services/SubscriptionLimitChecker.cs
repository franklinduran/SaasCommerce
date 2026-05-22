using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Services;

/// <summary>
/// Implements subscription limit checking against actual resource counts.
/// Validates that a business hasn't exceeded plan limits.
/// </summary>
public sealed class SubscriptionLimitChecker(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ISubscriptionUsageReader usageReader,
  IClock clock,
  ISubscriptionAccessPolicy accessPolicy) : ISubscriptionLimitChecker
{
  public async Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, subscription) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null || subscription is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.Branches, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    var currentBranches = await usageReader.CountActiveBranchesAsync(businessId, cancellationToken);

    if (currentBranches >= plan.MaxBranches)
    {
      return new SubscriptionLimitCheckResult(
        false,
        "SUBSCRIPTION_BRANCH_LIMIT_REACHED",
        $"Your plan allows {plan.MaxBranches} branch(es). You already have {currentBranches}.",
        currentBranches,
        plan.MaxBranches);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "Branch creation allowed.", currentBranches, plan.MaxBranches);
  }

  public async Task<SubscriptionLimitCheckResult> CanCreateUserAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, subscription) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null || subscription is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.Users, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    var currentUsers = await usageReader.CountActiveUsersAsync(businessId, cancellationToken);

    if (currentUsers >= plan.MaxUsers)
    {
      return new SubscriptionLimitCheckResult(
        false,
        "SUBSCRIPTION_USER_LIMIT_REACHED",
        $"Your plan allows {plan.MaxUsers} user(s). You already have {currentUsers}.",
        currentUsers,
        plan.MaxUsers);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "User creation allowed.", currentUsers, plan.MaxUsers);
  }

  public async Task<SubscriptionLimitCheckResult> CanCreateProductAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, subscription) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null || subscription is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.Products, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    var currentProducts = await usageReader.CountActiveProductsAsync(businessId, cancellationToken);

    if (currentProducts >= plan.MaxProducts)
    {
      return new SubscriptionLimitCheckResult(
        false,
        "SUBSCRIPTION_PRODUCT_LIMIT_REACHED",
        $"Your plan allows {plan.MaxProducts} product(s). You already have {currentProducts}.",
        currentProducts,
        plan.MaxProducts);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "Product creation allowed.", currentProducts, plan.MaxProducts);
  }

  public async Task<SubscriptionLimitCheckResult> CanCreateSaleAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, subscription) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null || subscription is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.Sales, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    var now = clock.UtcNow;
    var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
    var monthEnd = monthStart.AddMonths(1);

    var salesThisMonth = await usageReader.CountMonthlySalesAsync(
      businessId,
      monthStart,
      monthEnd,
      cancellationToken);

    if (salesThisMonth >= plan.MaxSalesPerMonth)
    {
      return new SubscriptionLimitCheckResult(
        false,
        "SUBSCRIPTION_SALES_LIMIT_REACHED",
        $"Your plan allows {plan.MaxSalesPerMonth} sale(s) per month. You've reached the limit.",
        salesThisMonth,
        plan.MaxSalesPerMonth);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "Sale creation allowed.", salesThisMonth, plan.MaxSalesPerMonth);
  }

  public async Task<SubscriptionLimitCheckResult> CanUseInventoryTransfersAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, _) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.InventoryTransfers, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "Inventory transfers allowed.", 0, 1);
  }

  public async Task<SubscriptionLimitCheckResult> CanUseAdvancedReportsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, _) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.Reports, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "Advanced reports allowed.", 0, 1);
  }

  public async Task<SubscriptionLimitCheckResult> CanUseAuditLogsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var (plan, _) = await GetPlanAndSubscriptionAsync(businessId, cancellationToken);
    if (plan is null)
    {
      return new SubscriptionLimitCheckResult(false, "NO_SUBSCRIPTION", "No active subscription found.", 0, 0);
    }

    var accessResult = await accessPolicy.EnsureCanUseFeatureAsync(businessId, SubscriptionFeature.AuditLogs, cancellationToken);
    if (accessResult.IsFailure)
    {
      return new SubscriptionLimitCheckResult(false, accessResult.Error.Code, accessResult.Error.Message, 0, 0);
    }

    return new SubscriptionLimitCheckResult(true, "OK", "Audit logs allowed.", 0, 1);
  }

  private async Task<(SubscriptionPlan? Plan, BusinessSubscription? Subscription)> GetPlanAndSubscriptionAsync(
    BusinessId businessId,
    CancellationToken cancellationToken)
  {
    var subscription = await subscriptionRepository.GetCurrentByBusinessIdAsync(businessId, cancellationToken);
    if (subscription is null)
    {
      return (null, null);
    }

    var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
    return (plan, subscription);
  }
}
