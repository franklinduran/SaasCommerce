using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public sealed record GetCashSessionDetailQuery(Guid CashSessionId);

public sealed class GetCashSessionDetailHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser)
{
  public async Task<Result<CashSessionResponse>> Handle(
    GetCashSessionDetailQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CashSessionResponse>(CashErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);

    var session = await cashSessions.GetAsync(bId, query.CashSessionId, cancellationToken);
    if (session is null)
    {
      return Result.Failure<CashSessionResponse>(CashErrors.SessionNotFound);
    }

    return Result.Success(CashSessionResponseMapper.ToResponse(session));
  }
}
