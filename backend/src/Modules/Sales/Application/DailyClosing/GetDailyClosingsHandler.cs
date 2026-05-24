using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public sealed record GetDailyClosingsQuery(
  Guid? BranchId,
  DateOnly? DateFrom,
  DateOnly? DateTo,
  int Page = 1,
  int PageSize = 20);

public sealed record DailyClosingsPagedResponse(
  IReadOnlyCollection<DailyClosingListItemResponse> Items,
  int TotalCount,
  int Page,
  int PageSize);

public sealed class GetDailyClosingsHandler(
  IDailyClosingReadRepository readRepository,
  ICurrentUserService currentUser)
{
  public async Task<Result<DailyClosingsPagedResponse>> Handle(
    GetDailyClosingsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<DailyClosingsPagedResponse>(DailyClosingErrors.UserContextRequired);
    }

    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);

    var (items, totalCount) = await readRepository.GetListAsync(
      new BusinessId(businessId),
      query.BranchId,
      query.DateFrom,
      query.DateTo,
      page,
      pageSize,
      cancellationToken);

    return Result.Success(new DailyClosingsPagedResponse(items, totalCount, page, pageSize));
  }
}
