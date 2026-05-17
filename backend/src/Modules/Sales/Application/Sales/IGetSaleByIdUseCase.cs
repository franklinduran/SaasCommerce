using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface IGetSaleByIdUseCase
{
  Task<Result<SaleResponse>> ExecuteAsync(
    GetSaleByIdQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record GetSaleByIdQuery(Guid SaleId);
