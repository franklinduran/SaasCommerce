using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Profitability;

public sealed record GetProductProfitabilityQuery(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo,
  Guid? BranchId,
  Guid? CategoryId);

public sealed class GetProductProfitabilityHandler(
  IProfitabilityReadRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<IReadOnlyCollection<ProductProfitabilityResponse>>> Handle(
    GetProductProfitabilityQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<IReadOnlyCollection<ProductProfitabilityResponse>>(
        ProfitabilityErrors.UserContextRequired);
    }

    var products = await repository.GetProductProfitabilityAsync(
      new BusinessId(businessId),
      query.DateFrom,
      query.DateTo,
      query.BranchId,
      query.CategoryId,
      cancellationToken);

    return Result.Success(products);
  }
}
