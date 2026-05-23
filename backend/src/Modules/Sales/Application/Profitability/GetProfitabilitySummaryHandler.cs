using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Profitability;

public sealed record GetProfitabilitySummaryQuery(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo,
  Guid? BranchId);

public sealed class GetProfitabilitySummaryHandler(
  IProfitabilityReadRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<ProfitabilitySummaryResponse>> Handle(
    GetProfitabilitySummaryQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProfitabilitySummaryResponse>(ProfitabilityErrors.UserContextRequired);
    }

    var summary = await repository.GetSummaryAsync(
      new BusinessId(businessId),
      query.DateFrom,
      query.DateTo,
      query.BranchId,
      cancellationToken);

    return Result.Success(summary);
  }
}
