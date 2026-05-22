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

    var businessId = command.BusinessId ?? currentUser.BusinessId ?? Guid.Empty;
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

    // Change the plan
    var now = clock.UtcNow;
    var newPeriodEnd = now.AddDays(BillingDays);
    var previousPlanId = subscription.PlanId;

    subscription.ChangePlan(command.NewPlanId, newPeriodEnd, now);

    await subscriptionRepository.UpdateAsync(subscription, cancellationToken);

    // Publish event for auditing
    await outbox.AddAsync(
      new BusinessSubscriptionChangedEventV1(
        businessId,
        subscription.Id,
        previousPlanId,
        command.NewPlanId,
        "Active", // Previous status (assumed active)
        "Active", // New status
        currentUser.UserId,
        $"Plan changed from {newPlan?.Name} to {newPlan?.Name}",
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var response = BusinessSubscriptionResponseMapper.ToResponse(subscription, newPlan!);
    return Result.Success(response);
  }
}
