using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class ReceivePurchaseHandler(PurchaseHandlerContext context, ICorrelationIdProvider correlationIdProvider)
{
  public Task<Result<PurchaseResponse>> Handle(
    ReceivePurchaseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<PurchaseResponse>> HandleCoreAsync(
    ReceivePurchaseCommand command,
    CancellationToken cancellationToken)
  {
    if (context.CurrentUser.BusinessId is not Guid businessId ||
        context.CurrentUser.UserId is not Guid)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var purchase = await context.Purchases.GetAsync(tenantId, command.PurchaseId, cancellationToken);

    if (purchase is null)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.PurchaseNotFound);
    }

    try
    {
      purchase.Receive(context.Clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.InvalidPurchaseState);
    }

    var correlationId = ResolveCorrelationId();
    await context.Outbox.AddAsync(
      CreatePurchaseHandler.ToReceivedEvent(purchase, correlationId),
      cancellationToken);
    await context.UnitOfWork.SaveChangesAsync(cancellationToken);

    var supplier = await context.Suppliers.GetAsync(tenantId, purchase.SupplierId, cancellationToken);
    var productMap = await context.Products.ListAsync(
      businessId,
      purchase.Items.Select(item => item.ProductId).Distinct().ToArray(),
      cancellationToken);

    return Result.Success(PurchaseResponseMapper.ToResponse(purchase, supplier?.Name, productMap));
  }

  private Guid ResolveCorrelationId()
    => Guid.TryParse(correlationIdProvider.CorrelationId, out var parsedValue)
      ? parsedValue
      : Guid.NewGuid();
}
