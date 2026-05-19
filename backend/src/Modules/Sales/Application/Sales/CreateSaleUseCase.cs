using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class CreateSaleUseCase(
  SaleHandlerContext context,
  ICustomerCreditRepository? customerCredits = null) : ICreateSaleUseCase
{
  public Task<Result<SaleResponse>> ExecuteAsync(
    CreateSaleCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return ExecuteCoreAsync(command, cancellationToken);
  }

  public async Task<Result> ExecuteAsync(
    SaleCreatedEventV1 saleCreated,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(saleCreated);

    var existingSale = await context.Sales.GetAsync(
      new BusinessId(saleCreated.BusinessId),
      saleCreated.SaleId,
      cancellationToken);

    if (existingSale is not null)
    {
      return Result.Success();
    }

    Sale sale;

    try
    {
      sale = Sale.Create(
        saleCreated.SaleId,
        new BusinessId(saleCreated.BusinessId),
        new BranchId(saleCreated.BranchId),
        saleCreated.UserId,
        saleCreated.Items.Select(item => new SaleLine(item.ProductId, item.Quantity, item.UnitPrice)).ToArray(),
        saleCreated.PaymentMethod,
        saleCreated.CreatedAt);

      sale.MarkAsProcessing(context.Clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure(SalesErrors.InvalidSale);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(SalesErrors.InvalidSale);
    }

    await context.Sales.AddAsync(sale, cancellationToken);
    await context.SaleEvents.AddAsync(saleCreated, cancellationToken);
    await context.UnitOfWork.SaveChangesAsync(cancellationToken);
    await context.SaleEvents.NotifyStatusChangedAsync(
      saleCreated,
      SaleStatus.Processing,
      null,
      cancellationToken);

    return Result.Success();
  }

  private async Task<Result<SaleResponse>> ExecuteCoreAsync(
    CreateSaleCommand command,
    CancellationToken cancellationToken)
  {
    var userContext = ResolveUserContext();

    if (userContext.IsFailure)
    {
      return Result.Failure<SaleResponse>(SalesErrors.UserContextRequired);
    }

    if (!IsValidCommand(command))
    {
      return Result.Failure<SaleResponse>(SalesErrors.InvalidSale);
    }

    var ctx = userContext.Value;
    var branchId = command.BranchId ?? ctx.BranchId;

    if (branchId != ctx.BranchId)
    {
      return Result.Failure<SaleResponse>(SalesErrors.InvalidSale);
    }

    var customerResult = await EnsureCustomerCanBeUsedAsync(
      new BusinessId(ctx.BusinessId),
      command.CustomerId,
      cancellationToken);

    if (customerResult.IsFailure)
    {
      return Result.Failure<SaleResponse>(customerResult.Error);
    }

    var linesResult = await BuildSaleLinesAsync(
      ctx.BusinessId,
      command.Items,
      cancellationToken);

    if (linesResult.IsFailure)
    {
      return Result.Failure<SaleResponse>(linesResult.Error);
    }

    var creditResult = await EnsureCreditSaleCanBeCreatedAsync(
      new BusinessId(ctx.BusinessId),
      command,
      linesResult.Value,
      cancellationToken);

    if (creditResult.IsFailure)
    {
      return Result.Failure<SaleResponse>(creditResult.Error);
    }

    var saleResult = CreateSale(
      command,
      ctx,
      branchId,
      linesResult.Value);

    if (saleResult.IsFailure)
    {
      return Result.Failure<SaleResponse>(saleResult.Error);
    }

    var sale = saleResult.Value;
    var saleCreated = await context.SaleEvents.AddSaleCreatedAsync(
      sale,
      ctx.BusinessId,
      branchId,
      ctx.UserId,
      cancellationToken);

    await context.Sales.AddAsync(sale, cancellationToken);
    await context.UnitOfWork.SaveChangesAsync(cancellationToken);
    await context.SaleEvents.NotifyStatusChangedAsync(
      saleCreated,
      SaleStatus.Received,
      null,
      cancellationToken);

    return Result.Success(SaleResponseMapper.ToResponse(sale));
  }

  private Result<SaleUserContext> ResolveUserContext()
  {
    if (context.CurrentUser.BusinessId is not Guid businessId ||
        context.CurrentUser.UserId is not Guid userId ||
        context.CurrentUser.BranchId is not Guid branchId)
    {
      return Result.Failure<SaleUserContext>(SalesErrors.UserContextRequired);
    }

    return Result.Success(new SaleUserContext(businessId, branchId, userId));
  }

  private static bool IsValidCommand(CreateSaleCommand command)
    => command.Items.Count > 0 &&
      !string.IsNullOrWhiteSpace(command.PaymentMethod);

  private async Task<Result> EnsureCustomerCanBeUsedAsync(
    BusinessId businessId,
    Guid? customerId,
    CancellationToken cancellationToken)
  {
    if (!customerId.HasValue)
    {
      return Result.Success();
    }

    var customer = await context.Customers.GetAsync(
      businessId,
      customerId.Value,
      cancellationToken);

    return customer is null || !customer.IsActive
      ? Result.Failure(SalesErrors.CustomerNotFound)
      : Result.Success();
  }

  private async Task<Result<IReadOnlyCollection<SaleLine>>> BuildSaleLinesAsync(
    Guid businessId,
    IReadOnlyCollection<CreateSaleItemCommand> items,
    CancellationToken cancellationToken)
  {
    var lines = new List<SaleLine>(items.Count);

    foreach (var item in items)
    {
      var lineResult = await CreateSaleLineAsync(
        businessId,
        item,
        cancellationToken);

      if (lineResult.IsFailure)
      {
        return Result.Failure<IReadOnlyCollection<SaleLine>>(lineResult.Error);
      }

      lines.Add(lineResult.Value);
    }

    return Result.Success<IReadOnlyCollection<SaleLine>>(lines);
  }

  private async Task<Result> EnsureCreditSaleCanBeCreatedAsync(
    BusinessId businessId,
    CreateSaleCommand command,
    IReadOnlyCollection<SaleLine> lines,
    CancellationToken cancellationToken)
  {
    if (!RegisterCreditSaleUseCase.IsCreditSale(command.PaymentMethod))
    {
      return Result.Success();
    }

    if (command.CustomerId is not Guid customerId)
    {
      return Result.Failure(SalesErrors.InvalidSale);
    }

    if (customerCredits is null)
    {
      return Result.Failure(CustomerCreditErrors.InvalidCreditOperation);
    }

    var account = await customerCredits.GetAccountAsync(
      businessId,
      customerId,
      cancellationToken);

    if (account is null)
    {
      account = new CustomerCreditAccount(Guid.NewGuid(), businessId, customerId, 0, context.Clock.UtcNow);
      await customerCredits.AddAccountAsync(account, cancellationToken);
    }

    var total = lines.Sum(line => line.Quantity * line.UnitPrice);

    try
    {
      account.EnsureCanDebit(total);
    }
    catch (InvalidOperationException) when (account.Status is CustomerCreditStatus.Blocked or CustomerCreditStatus.Closed)
    {
      return Result.Failure(CustomerCreditErrors.CreditAccountBlocked);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(CustomerCreditErrors.CreditLimitExceeded);
    }
    catch (ArgumentOutOfRangeException)
    {
      return Result.Failure(CustomerCreditErrors.InvalidCreditOperation);
    }

    return Result.Success();
  }

  private async Task<Result<SaleLine>> CreateSaleLineAsync(
    Guid businessId,
    CreateSaleItemCommand item,
    CancellationToken cancellationToken)
  {
    if (item.ProductId == Guid.Empty || item.Quantity <= 0)
    {
      return Result.Failure<SaleLine>(SalesErrors.InvalidSale);
    }

    var productPolicy = await context.ProductPolicies.GetSalesPolicyAsync(
      businessId,
      item.ProductId,
      cancellationToken);

    if (productPolicy is null || !productPolicy.CanBeSold)
    {
      return Result.Failure<SaleLine>(SalesErrors.ProductNotFound);
    }

    return Result.Success(new SaleLine(
      item.ProductId,
      item.Quantity,
      productPolicy.SalePrice));
  }

  private Result<Sale> CreateSale(
    CreateSaleCommand command,
    SaleUserContext ctx,
    Guid branchId,
    IReadOnlyCollection<SaleLine> lines)
  {
    try
    {
      var sale = Sale.Create(
        Guid.NewGuid(),
        new BusinessId(ctx.BusinessId),
        new BranchId(branchId),
        ctx.UserId,
        lines,
        command.PaymentMethod,
        context.Clock.UtcNow);

      if (command.CustomerId.HasValue)
      {
        sale.AssignCustomer(command.CustomerId.Value);
      }

      return Result.Success(sale);
    }
    catch (ArgumentException)
    {
      return Result.Failure<Sale>(SalesErrors.InvalidSale);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<Sale>(SalesErrors.InvalidSale);
    }
  }

  private sealed record SaleUserContext(Guid BusinessId, Guid BranchId, Guid UserId);
}
