using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class GetSaleReturnByIdHandler(
  ISaleReturnReadRepository returns,
  ICurrentUserService currentUser)
{
  public async Task<Result<SaleReturnResponse>> Handle(
    GetSaleReturnByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<SaleReturnResponse>(SalesErrors.UserContextRequired);
    }

    var response = await returns.GetAsync(new BusinessId(businessId), query.SaleReturnId, cancellationToken);
    return response is null
      ? Result.Failure<SaleReturnResponse>(SalesErrors.SaleReturnNotFound)
      : Result.Success(response);
  }
}

public sealed class ListSaleReturnsHandler(
  ISaleReturnReadRepository returns,
  ICurrentUserService currentUser)
{
  public async Task<Result<IReadOnlyCollection<SaleReturnResponse>>> Handle(
    ListSaleReturnsQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<IReadOnlyCollection<SaleReturnResponse>>(SalesErrors.UserContextRequired);
    }

    var items = await returns.ListBySaleAsync(new BusinessId(businessId), query.SaleId, cancellationToken);
    return Result.Success(items);
  }
}
