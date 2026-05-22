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
/// Handler for ReactivateBusinessSubscriptionCommand.
/// Reactivates a cancelled subscription.
/// Can optionally change the plan during reactivation.
/// Publishes BusinessSubscriptionChangedEventV1 for auditing.
/// </summary>
public sealed class ReactivateBusinessSubscriptionCommandHandler(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IOutboxWriter outbox)
{
  private const int BillingDays = 30;

  public async Task<Result<BusinessSubscriptionResponse>> Handle(
    ReactivateBusinessSubscriptionCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

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

    // Only cancelled subscriptions can be reactivated
    if (subscription.Status.ToString() != "Cancelled")
    {
      return Result.Failure<BusinessSubscriptionResponse>(
        new DomainError("subscription.not_cancelled", "Solo se pueden reactivar suscripciones canceladas"));
    }

    var now = clock.UtcNow;
    var newPeriodEnd = now.AddDays(BillingDays);
    var previousPlanId = subscription.PlanId;
    var previousStatus = subscription.Status.ToString();

    // If a new plan is specified, change it; otherwise keep the original plan
    if (command.NewPlanId.HasValue)
    {
      var newPlan = await planRepository.GetByIdAsync(command.NewPlanId.Value, cancellationToken);
      if (newPlan is null)
      {
        return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotFound);
      }

      if (!newPlan.IsActive)
      {
        return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotActive);
      }

      subscription.ReactivateWithPlanChange(command.NewPlanId.Value, newPeriodEnd, now);
    }
    else
    {
      // Reactivate with original plan
      subscription.Reactivate(newPeriodEnd, now);
    }

    await subscriptionRepository.UpdateAsync(subscription, cancellationToken);

    // Get the current plan for the response
    var currentPlan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
    if (currentPlan is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotFound);
    }

    // Publish event for auditing
    await outbox.AddAsync(
      new BusinessSubscriptionChangedEventV1(
        businessId,
        subscription.Id,
        previousPlanId,
        subscription.PlanId,
        previousStatus,
        "Active",
        currentUser.UserId,
        command.NewPlanId.HasValue ? "Subscription reactivated with plan change" : "Subscription reactivated",
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var response = BusinessSubscriptionResponseMapper.ToResponse(subscription, currentPlan);
    return Result.Success(response);
  }
}
