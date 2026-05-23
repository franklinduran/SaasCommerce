using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Profitability;

public sealed record GetBranchProfitabilityQuery(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo);

public sealed class GetBranchProfitabilityHandler(
  IProfitabilityReadRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<IReadOnlyCollection<BranchProfitabilityResponse>>> Handle(
    GetBranchProfitabilityQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<IReadOnlyCollection<BranchProfitabilityResponse>>(
        ProfitabilityErrors.UserContextRequired);
    }

    var branches = await repository.GetBranchProfitabilityAsync(
      new BusinessId(businessId),
      query.DateFrom,
      query.DateTo,
      cancellationToken);

    return Result.Success(branches);
  }
}
