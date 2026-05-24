using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public sealed record GetDailyClosingDetailQuery(Guid ClosingId);

public sealed class GetDailyClosingDetailHandler(
  IDailyClosingReadRepository readRepository,
  ICurrentUserService currentUser)
{
  public async Task<Result<DailyClosingDetailResponse>> Handle(
    GetDailyClosingDetailQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.UserContextRequired);
    }

    var detail = await readRepository.GetDetailAsync(
      query.ClosingId,
      new BusinessId(businessId),
      cancellationToken);

    if (detail is null)
    {
      return Result.Failure<DailyClosingDetailResponse>(DailyClosingErrors.NotFound);
    }

    return Result.Success(detail);
  }
}
