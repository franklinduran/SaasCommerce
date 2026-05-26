using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public sealed class GetActiveCashRegisterHandler(
  ICashRegisterRepository registers,
  ICurrentUserService currentUser)
{
  public async Task<Result<CashRegisterDetailResponse?>> Handle(CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId || currentUser.UserId is not Guid userId)
    {
      return Result.Failure<CashRegisterDetailResponse?>(CashRegisterErrors.UserContextRequired);
    }

    var businessId = new BusinessId(rawBusinessId);
    var register = await registers.GetOpenRegisterAsync(businessId, userId, cancellationToken);

    if (register is null)
    {
      return Result.Success<CashRegisterDetailResponse?>(null);
    }

    return Result.Success<CashRegisterDetailResponse?>(CashRegisterResponseMapper.ToDetail(register));
  }
}
