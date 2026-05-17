using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface ICancelSaleUseCase
{
  Task<Result<SaleResponse>> ExecuteAsync(
    CancelSaleCommand command,
    CancellationToken cancellationToken = default);
}

public sealed record CancelSaleCommand(Guid SaleId, string? Reason);
