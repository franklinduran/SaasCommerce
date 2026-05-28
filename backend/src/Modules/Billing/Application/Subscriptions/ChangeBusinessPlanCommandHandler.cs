using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Mappers;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Handler for ChangeBusinessPlanCommand.
/// Changes the plan for a business subscription.
/// Extends the billing period to 30 days from now.
/// Publishes BusinessSubscriptionChangedEventV1 for auditing.
/// </summary>
public sealed class ChangeBusinessPlanCommandHandler(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ISubscriptionUsageReader usageReader,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IOutboxWriter outbox)
{
  private const int BillingDays = 30;

  public async Task<Result<BusinessSubscriptionResponse>> Handle(
    ChangeBusinessPlanCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (command.NewPlanId == Guid.Empty)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.InvalidPlanData);
    }

    var businessId = currentUser.BusinessId ?? Guid.Empty;
    if (businessId == Guid.Empty)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);

    // Get current subscription
    var subscription = await subscriptionRepository.GetCurrentByBusinessIdAsync(tenantId, cancellationToken);
    if (subscription is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.SubscriptionNotFound);
    }

    // Verify new plan exists and is active
    var newPlan = await planRepository.GetByIdAsync(command.NewPlanId, cancellationToken);
    if (newPlan is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotFound);
    }

    if (!newPlan.IsActive)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotActive);
    }

    var now = clock.UtcNow;
    var downgradeCheck = await EnsureCurrentUsageFitsPlanAsync(tenantId, newPlan, now, cancellationToken);
    if (downgradeCheck.IsFailure)
    {
      return Result.Failure<BusinessSubscriptionResponse>(downgradeCheck.Error);
    }

    // Capture previous state before mutating
    var newPeriodEnd = now.AddDays(BillingDays);
    var previousPlanId = subscription.PlanId;
    var previousStatus = subscription.Status.ToString();

    subscription.ChangePlan(command.NewPlanId, newPeriodEnd, now);

    await subscriptionRepository.UpdateAsync(subscription, cancellationToken);

    // Publish event for auditing
    await outbox.AddAsync(
      new BusinessSubscriptionChangedEventV1(
        businessId,
        subscription.Id,
        previousPlanId,
        command.NewPlanId,
        previousStatus,
        subscription.Status.ToString(),
        currentUser.UserId,
        $"Plan changed to {newPlan!.Name}",
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var response = BusinessSubscriptionResponseMapper.ToResponse(subscription, newPlan!);
    return Result.Success(response);
  }

  private async Task<Result> EnsureCurrentUsageFitsPlanAsync(
    BusinessId businessId,
    SubscriptionPlan plan,
    DateTimeOffset now,
    CancellationToken cancellationToken)
  {
    var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
    var monthEnd = monthStart.AddMonths(1);
    var violations = new List<string>();

    var branches = await usageReader.CountActiveBranchesAsync(businessId, cancellationToken);
    if (branches > plan.MaxBranches)
    {
      violations.Add($"sucursales activas: {branches}/{plan.MaxBranches}");
    }

    var users = await usageReader.CountActiveUsersAsync(businessId, cancellationToken);
    if (users > plan.MaxUsers)
    {
      violations.Add($"usuarios activos: {users}/{plan.MaxUsers}");
    }

    var products = await usageReader.CountActiveProductsAsync(businessId, cancellationToken);
    if (products > plan.MaxProducts)
    {
      violations.Add($"productos activos: {products}/{plan.MaxProducts}");
    }

    var monthlySales = await usageReader.CountMonthlySalesAsync(businessId, monthStart, monthEnd, cancellationToken);
    if (monthlySales > plan.MaxSalesPerMonth)
    {
      violations.Add($"ventas del mes: {monthlySales}/{plan.MaxSalesPerMonth}");
    }

    if (violations.Count == 0)
    {
      return Result.Success();
    }

    return Result.Failure(SubscriptionErrors.DowngradeBlocked(
      $"No puedes cambiar a {plan.Name} porque tu uso actual excede sus limites: {string.Join(", ", violations)}. Reduce el uso o elige un plan superior."));
  }
}
