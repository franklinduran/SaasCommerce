using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Mappers;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Billing.Application.Subscriptions.Plans;

/// <summary>
/// Handler for GetSubscriptionPlansQuery.
/// Returns all active subscription plans.
/// </summary>
public sealed class GetSubscriptionPlansQueryHandler(ISubscriptionPlanRepository planRepository)
 
{
  public async Task<Result<List<SubscriptionPlanResponse>>> Handle(
    GetSubscriptionPlansQuery query,
    CancellationToken cancellationToken = default)
  {
    var plans = await planRepository.GetActiveAsync(cancellationToken);

    var responses = plans
      .Select(SubscriptionPlanResponseMapper.ToResponse)
      .ToList();

    return Result.Success(responses);
  }
}
