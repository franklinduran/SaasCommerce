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
/// Handler for CancelBusinessSubscriptionCommand.
/// Cancels a subscription immediately (final state).
/// Publishes BusinessSubscriptionChangedEventV1 for auditing.
/// </summary>
public sealed class CancelBusinessSubscriptionCommandHandler(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IOutboxWriter outbox)
{
  public async Task<Result<BusinessSubscriptionResponse>> Handle(
    CancelBusinessSubscriptionCommand command,
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

    // Cancel the subscription
    var now = clock.UtcNow;
    var previousStatus = subscription.Status.ToString();
    subscription.Cancel(now);

    await subscriptionRepository.UpdateAsync(subscription, cancellationToken);

    // Get the plan for the response (even though cancelled)
    var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotFound);
    }

    // Publish event for auditing
    await outbox.AddAsync(
      new BusinessSubscriptionChangedEventV1(
        businessId,
        subscription.Id,
        subscription.PlanId,
        subscription.PlanId,
        previousStatus,
        "Cancelled",
        currentUser.UserId,
        "Subscription cancelled by user",
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var response = BusinessSubscriptionResponseMapper.ToResponse(subscription, plan);
    return Result.Success(response);
  }
}
