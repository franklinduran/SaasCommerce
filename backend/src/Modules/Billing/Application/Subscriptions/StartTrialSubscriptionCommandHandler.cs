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
/// Handler for StartTrialSubscriptionCommand.
/// Creates a new trial subscription for a business.
/// Trial duration is 14 days by default.
/// Publishes BusinessSubscriptionChangedEventV1 for auditing.
/// </summary>
public sealed class StartTrialSubscriptionCommandHandler(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IOutboxWriter outbox)
{
  private const int TrialDays = 14;

  public async Task<Result<BusinessSubscriptionResponse>> Handle(
    StartTrialSubscriptionCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var businessId = command.BusinessId ?? currentUser.BusinessId ?? Guid.Empty;

    if (businessId == Guid.Empty)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);

    // Check if subscription already exists
    var existing = await subscriptionRepository.GetCurrentByBusinessIdAsync(tenantId, cancellationToken);
    if (existing is not null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.DuplicateSubscription);
    }

    // Get the Basic plan for the trial
    var basicPlan = await planRepository.GetByCodeAsync(SubscriptionPlanCodes.Basic, cancellationToken);
    if (basicPlan is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotFound);
    }

    // Create trial subscription
    var now = clock.UtcNow;
    var trialEndsAt = now.AddDays(TrialDays);

    var subscription = BusinessSubscription.StartTrial(
      Guid.NewGuid(),
      tenantId,
      basicPlan.Id,
      now,
      trialEndsAt);

    await subscriptionRepository.AddAsync(subscription, cancellationToken);

    // Publish event for auditing
    await outbox.AddAsync(
      new BusinessSubscriptionChangedEventV1(
        businessId,
        subscription.Id,
        null,
        basicPlan.Id,
        "None",
        "Trial",
        currentUser.UserId,
        "Trial subscription started",
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var response = BusinessSubscriptionResponseMapper.ToResponse(subscription, basicPlan);
    return Result.Success(response);
  }
}
