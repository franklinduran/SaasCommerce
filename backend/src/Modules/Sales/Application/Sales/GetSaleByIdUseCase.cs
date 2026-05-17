using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class GetSaleByIdUseCase(
  ISaleReadRepository sales,
  ICurrentUserService currentUser) : IGetSaleByIdUseCase
{
  public Task<Result<SaleResponse>> ExecuteAsync(
    GetSaleByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return ExecuteCoreAsync(query, cancellationToken);
  }

  private async Task<Result<SaleResponse>> ExecuteCoreAsync(
    GetSaleByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<SaleResponse>(SalesErrors.UserContextRequired);
    }

    var sale = await sales.GetAsync(
      new BusinessId(businessId),
      query.SaleId,
      cancellationToken);

    return sale is null
      ? Result.Failure<SaleResponse>(SalesErrors.SaleNotFound)
      : Result.Success(sale);
  }
}
