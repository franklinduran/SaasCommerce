using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public sealed record GetCashSessionsQuery(
  Guid? BranchId,
  string? Status,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page = 1,
  int PageSize = 20);

public sealed record CashSessionsListResult(
  IReadOnlyCollection<CashSessionListResponse> Items,
  int TotalCount,
  int Page,
  int PageSize);

public sealed class GetCashSessionsHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser)
{
  public async Task<Result<CashSessionsListResult>> Handle(
    GetCashSessionsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CashSessionsListResult>(CashErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);
    var criteria = new CashSessionSearchCriteria(
      query.BranchId,
      query.Status,
      query.DateFrom,
      query.DateTo,
      Math.Max(1, query.Page),
      Math.Clamp(query.PageSize, 1, 100));

    var totalCount = await cashSessions.CountAsync(bId, criteria, cancellationToken);
    var sessions = await cashSessions.ListAsync(bId, criteria, cancellationToken);

    var items = sessions.Select(CashSessionResponseMapper.ToListResponse).ToArray();

    return Result.Success(new CashSessionsListResult(items, totalCount, criteria.Page, criteria.PageSize));
  }
}
