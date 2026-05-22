using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public interface ICreateSaleUseCase
{
  Task<Result<SaleResponse>> ExecuteAsync(
    CreateSaleCommand command,
    CancellationToken cancellationToken = default);

  Task<Result> ExecuteAsync(
    SaleCreatedEventV1 saleCreated,
    CancellationToken cancellationToken = default);
}

public sealed record CreateSaleCommand(
  Guid? BranchId,
  Guid? CustomerId,
  string PaymentMethod,
  IReadOnlyCollection<CreateSaleItemCommand> Items,
  Guid? BusinessId = null);

public sealed record CreateSaleItemCommand(
  Guid ProductId,
  decimal Quantity);
