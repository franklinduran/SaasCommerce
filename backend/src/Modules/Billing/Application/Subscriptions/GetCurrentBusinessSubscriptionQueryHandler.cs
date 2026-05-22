using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Mappers;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Subscriptions;

/// <summary>
/// Handler for GetCurrentBusinessSubscriptionQuery.
/// Returns the active subscription for the authenticated business.
/// </summary>
public sealed class GetCurrentBusinessSubscriptionQueryHandler(
  IBusinessSubscriptionRepository subscriptionRepository,
  ISubscriptionPlanRepository planRepository,
  ICurrentUserService currentUser)
{
  public async Task<Result<BusinessSubscriptionResponse>> Handle(
    GetCurrentBusinessSubscriptionQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId == null || currentUser.BusinessId == Guid.Empty)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.UserContextRequired);
    }

    var businessId = new BusinessId(currentUser.BusinessId.Value);
    var subscription = await subscriptionRepository.GetCurrentByBusinessIdAsync(businessId, cancellationToken);

    if (subscription is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.SubscriptionNotFound);
    }

    var plan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure<BusinessSubscriptionResponse>(SubscriptionErrors.PlanNotFound);
    }

    var response = BusinessSubscriptionResponseMapper.ToResponse(subscription, plan);
    return Result.Success(response);
  }
}
