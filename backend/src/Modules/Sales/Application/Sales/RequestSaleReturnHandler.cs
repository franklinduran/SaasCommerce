using FluentValidation;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class RequestSaleReturnHandler(
  RequestSaleReturnDependencies dependencies,
  IValidator<RequestSaleReturnCommand> validator) : IRequestSaleReturnUseCase
{
  private readonly ISaleRepository sales = dependencies.Sales;
  private readonly ISaleReturnRepository returns = dependencies.Returns;
  private readonly ISaleReturnReadRepository returnReads = dependencies.ReturnReads;
  private readonly ICurrentUserService currentUser = dependencies.CurrentUser;
  private readonly IOutboxWriter outbox = dependencies.Outbox;
  private readonly ICorrelationIdProvider correlationIdProvider = dependencies.CorrelationIdProvider;
  private readonly IClock clock = dependencies.Clock;
  private readonly IUnitOfWork unitOfWork = dependencies.UnitOfWork;

  public async Task<Result<SaleReturnResponse>> ExecuteAsync(
    RequestSaleReturnCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var validation = await validator.ValidateAsync(command, cancellationToken);
    if (!validation.IsValid)
    {
      return Result.Failure<SaleReturnResponse>(SalesErrors.InvalidSaleReturn);
    }

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<SaleReturnResponse>(SalesErrors.UserContextRequired);
    }

    var tenant = new BusinessId(businessId);
    var sale = await sales.GetAsync(tenant, command.SaleId, cancellationToken);
    if (sale is null)
    {
      return Result.Failure<SaleReturnResponse>(SalesErrors.SaleNotFound);
    }

    var linesResult = await BuildLinesAsync(tenant, sale, command.Items, cancellationToken);
    if (linesResult.IsFailure)
    {
      return Result.Failure<SaleReturnResponse>(linesResult.Error);
    }

    SaleReturn saleReturn;
    try
    {
      saleReturn = SaleReturn.Request(
        Guid.NewGuid(),
        sale,
        userId,
        command.Reason,
        linesResult.Value,
        clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure<SaleReturnResponse>(SalesErrors.InvalidSaleReturn);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<SaleReturnResponse>(SalesErrors.InvalidSaleReturn);
    }

    await returns.AddAsync(saleReturn, cancellationToken);
    await outbox.AddAsync(
      new SaleReturnRequestedEventV1(
        Guid.NewGuid(),
        ResolveCorrelationId(),
        businessId,
        sale.BranchId.Value,
        sale.Id,
        saleReturn.Id,
        userId,
        SaleReturnMapper.ToEventItems(saleReturn),
        saleReturn.Total,
        saleReturn.Reason,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    var response = await returnReads.GetAsync(tenant, saleReturn.Id, cancellationToken);
    return Result.Success(response ?? SaleReturnMapper.ToResponse(saleReturn));
  }

  private async Task<Result<IReadOnlyCollection<SaleReturnLine>>> BuildLinesAsync(
    BusinessId businessId,
    Sale sale,
    IReadOnlyCollection<RequestSaleReturnItemCommand> requestedItems,
    CancellationToken cancellationToken)
  {
    var alreadyReturned = await returns.GetReturnedQuantityBySaleItemAsync(
      businessId,
      sale.Id,
      cancellationToken);

    var lines = new List<SaleReturnLine>(requestedItems.Count);
    foreach (var item in requestedItems)
    {
      var saleItem = sale.Items.SingleOrDefault(s => s.Id == item.SaleItemId);
      if (saleItem is null)
      {
        return Result.Failure<IReadOnlyCollection<SaleReturnLine>>(SalesErrors.InvalidSaleReturn);
      }

      var returned = alreadyReturned.GetValueOrDefault(item.SaleItemId);
      if (item.Quantity > saleItem.Quantity - returned)
      {
        return Result.Failure<IReadOnlyCollection<SaleReturnLine>>(SalesErrors.InvalidSaleReturn);
      }

      lines.Add(new SaleReturnLine(item.SaleItemId, item.Quantity));
    }

    return Result.Success<IReadOnlyCollection<SaleReturnLine>>(lines);
  }

  private Guid ResolveCorrelationId()
    => Guid.TryParse(correlationIdProvider.CorrelationId, out var correlationId)
      ? correlationId
      : Guid.NewGuid();
}

public sealed class RequestSaleReturnDependencies
{
  public required ISaleRepository Sales { get; init; }
  public required ISaleReturnRepository Returns { get; init; }
  public required ISaleReturnReadRepository ReturnReads { get; init; }
  public required ICurrentUserService CurrentUser { get; init; }
  public required IOutboxWriter Outbox { get; init; }
  public required ICorrelationIdProvider CorrelationIdProvider { get; init; }
  public required IClock Clock { get; init; }
  public required IUnitOfWork UnitOfWork { get; init; }
}
