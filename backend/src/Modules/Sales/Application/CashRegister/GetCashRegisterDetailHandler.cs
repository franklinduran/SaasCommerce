using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public sealed record GetCashRegisterDetailQuery(Guid CashRegisterId);

public sealed class GetCashRegisterDetailHandler(
  ICashRegisterReadRepository registers,
  ICurrentUserService currentUser)
{
  public async Task<Result<CashRegisterDetailResponse>> Handle(
    GetCashRegisterDetailQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId)
    {
      return Result.Failure<CashRegisterDetailResponse>(CashRegisterErrors.UserContextRequired);
    }

    var businessId = new BusinessId(rawBusinessId);
    var register = await registers.GetAsync(businessId, query.CashRegisterId, cancellationToken);
    if (register is null)
    {
      return Result.Failure<CashRegisterDetailResponse>(CashRegisterErrors.RegisterNotFound);
    }

    return Result.Success(CashRegisterResponseMapper.ToDetail(register));
  }
}
