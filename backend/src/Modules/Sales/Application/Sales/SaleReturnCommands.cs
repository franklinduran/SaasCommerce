using FluentValidation;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed record RequestSaleReturnCommand(
  Guid SaleId,
  string Reason,
  IReadOnlyCollection<RequestSaleReturnItemCommand> Items);

public sealed record RequestSaleReturnItemCommand(Guid SaleItemId, decimal Quantity);

public sealed record GetSaleReturnByIdQuery(Guid SaleReturnId);

public sealed record ListSaleReturnsQuery(Guid SaleId);

public interface IRequestSaleReturnUseCase
{
  Task<Result<SaleReturnResponse>> ExecuteAsync(
    RequestSaleReturnCommand command,
    CancellationToken cancellationToken = default);
}

public interface IApproveSaleReturnUseCase
{
  Task<Result> ExecuteAsync(
    SaleReturnRequestedEventV1 message,
    CancellationToken cancellationToken = default);
}

public interface IRestoreInventoryFromSaleReturnUseCase
{
  Task<Result> ExecuteAsync(
    SaleReturnApprovedEventV1 message,
    CancellationToken cancellationToken = default);
}

public interface IGenerateCreditNoteForReturnUseCase
{
  Task<Result> ExecuteAsync(
    SaleReturnApprovedEventV1 message,
    CancellationToken cancellationToken = default);
}

public sealed class RequestSaleReturnCommandValidator : AbstractValidator<RequestSaleReturnCommand>
{
  public RequestSaleReturnCommandValidator()
  {
    RuleFor(command => command.SaleId).NotEmpty();
    RuleFor(command => command.Reason).NotEmpty().MaximumLength(500);
    RuleFor(command => command.Items).NotEmpty();
    RuleForEach(command => command.Items).ChildRules(item =>
    {
      item.RuleFor(i => i.SaleItemId).NotEmpty();
      item.RuleFor(i => i.Quantity).GreaterThan(0);
    });
  }
}
