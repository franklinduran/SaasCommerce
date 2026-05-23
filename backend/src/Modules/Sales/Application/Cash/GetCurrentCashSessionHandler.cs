using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public sealed class GetCurrentCashSessionHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser)
{
  public async Task<Result<CashSessionResponse?>> Handle(CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.BranchId is not Guid branchIdValue)
    {
      return Result.Failure<CashSessionResponse?>(CashErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);
    var branchId = new BranchId(branchIdValue);

    var session = await cashSessions.GetOpenSessionAsync(bId, branchId, cancellationToken);

    return Result.Success(session is null ? null : CashSessionResponseMapper.ToResponse(session));
  }
}
