using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class CreatePurchaseHandler(
  PurchaseHandlerContext context,
  PurchaseReceiptProcessor receiptProcessor,
  ICorrelationIdProvider correlationIdProvider,
  ISubscriptionAccessPolicy subscriptionAccess)
{
  public Task<Result<PurchaseResponse>> Handle(
    CreatePurchaseCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<PurchaseResponse>> HandleCoreAsync(
    CreatePurchaseCommand command,
    CancellationToken cancellationToken)
  {
    var userContext = ResolveUserContext();

    if (userContext.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.UserContextRequired);
    }

    var ctx = userContext.Value;
    var branchId = command.BranchId ?? ctx.BranchId;
    var tenantId = new BusinessId(ctx.BusinessId);

    var access = await subscriptionAccess.EnsureCanUseFeatureAsync(
      tenantId,
      SubscriptionFeatures.Purchases,
      cancellationToken);
    if (access.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(access.Error);
    }

    if (!IsValidCommand(command) || branchId == Guid.Empty)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.InvalidPurchase);
    }

    var supplier = await context.Suppliers.GetAsync(
      tenantId,
      command.SupplierId,
      cancellationToken);

    if (supplier is null || !supplier.IsActive)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.SupplierNotFound);
    }

    var lines = await BuildPurchaseLinesAsync(
      ctx.BusinessId,
      command.Items,
      cancellationToken);

    if (lines.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(lines.Error);
    }

    var purchase = CreatePurchase(command, ctx, branchId, lines.Value);

    if (purchase.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(purchase.Error);
    }

    var entity = purchase.Value;
    var correlationId = ResolveCorrelationId();
    await context.Purchases.AddAsync(entity, cancellationToken);
    await context.Outbox.AddAsync(
      ToCreatedEvent(entity, correlationId),
      cancellationToken);

    if (command.ReceiveNow)
    {
      var receipt = await receiptProcessor.ProcessAsync(
        entity,
        ctx.UserId,
        correlationId,
        markAsReceived: true,
        cancellationToken);

      if (receipt.IsFailure)
      {
        return Result.Failure<PurchaseResponse>(receipt.Error);
      }

      await context.Outbox.AddAsync(ToReceivedEvent(entity, correlationId), cancellationToken);
    }

    await context.UnitOfWork.SaveChangesAsync(cancellationToken);

    var productMap = await context.Products.ListAsync(
      ctx.BusinessId,
      entity.Items.Select(item => item.ProductId).Distinct().ToArray(),
      cancellationToken);

    return Result.Success(PurchaseResponseMapper.ToResponse(entity, supplier.Name, productMap));
  }

  private Result<PurchaseUserContext> ResolveUserContext()
  {
    if (context.CurrentUser.BusinessId is not Guid businessId ||
        context.CurrentUser.UserId is not Guid userId ||
        context.CurrentUser.BranchId is not Guid branchId)
    {
      return Result.Failure<PurchaseUserContext>(PurchaseErrors.UserContextRequired);
    }

    return Result.Success(new PurchaseUserContext(businessId, branchId, userId));
  }

  private static bool IsValidCommand(CreatePurchaseCommand command)
    => command.SupplierId != Guid.Empty && command.Items.Count > 0;

  private async Task<Result<IReadOnlyCollection<PurchaseLine>>> BuildPurchaseLinesAsync(
    Guid businessId,
    IReadOnlyCollection<CreatePurchaseItemCommand> items,
    CancellationToken cancellationToken)
  {
    var lines = new List<PurchaseLine>(items.Count);

    foreach (var item in items)
    {
      var line = await CreatePurchaseLineAsync(businessId, item, cancellationToken);

      if (line.IsFailure)
      {
        return Result.Failure<IReadOnlyCollection<PurchaseLine>>(line.Error);
      }

      lines.Add(line.Value);
    }

    return Result.Success<IReadOnlyCollection<PurchaseLine>>(lines);
  }

  private async Task<Result<PurchaseLine>> CreatePurchaseLineAsync(
    Guid businessId,
    CreatePurchaseItemCommand item,
    CancellationToken cancellationToken)
  {
    if (item.ProductId == Guid.Empty || item.Quantity <= 0 || item.UnitCost < 0)
    {
      return Result.Failure<PurchaseLine>(PurchaseErrors.InvalidPurchase);
    }

    var product = await context.Products.GetAsync(businessId, item.ProductId, cancellationToken);

    if (product is null || !product.IsActive)
    {
      return Result.Failure<PurchaseLine>(PurchaseErrors.ProductNotFound);
    }

    if (!product.TrackInventory)
    {
      return Result.Failure<PurchaseLine>(PurchaseErrors.ProductDoesNotTrackInventory);
    }

    return Result.Success(new PurchaseLine(item.ProductId, item.Quantity, item.UnitCost));
  }

  private Result<Purchase> CreatePurchase(
    CreatePurchaseCommand command,
    PurchaseUserContext ctx,
    Guid branchId,
    IReadOnlyCollection<PurchaseLine> lines)
  {
    try
    {
      var now = context.Clock.UtcNow;
      return Result.Success(Purchase.Create(
        new PurchaseCreationData(
          Guid.NewGuid(),
          new BusinessId(ctx.BusinessId),
          new BranchId(branchId),
          command.SupplierId,
          ctx.UserId,
          command.SupplierInvoiceNumber,
          command.PurchaseDate ?? now,
          command.Notes,
          now),
        lines));
    }
    catch (ArgumentOutOfRangeException)
    {
      return Result.Failure<Purchase>(PurchaseErrors.InvalidPurchase);
    }
    catch (ArgumentException)
    {
      return Result.Failure<Purchase>(PurchaseErrors.InvalidPurchase);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<Purchase>(PurchaseErrors.InvalidPurchase);
    }
  }

  private static PurchaseCreatedEventV1 ToCreatedEvent(Purchase purchase, Guid correlationId)
    => new(
      Guid.NewGuid(),
      correlationId,
      purchase.Id,
      purchase.BusinessId.Value,
      purchase.BranchId.Value,
      purchase.SupplierId,
      purchase.UserId,
      ToEventItems(purchase),
      purchase.Total,
      purchase.Status.ToString(),
      purchase.CreatedAt);

  internal static PurchaseReceivedEventV1 ToReceivedEvent(Purchase purchase, Guid correlationId)
    => new(
      Guid.NewGuid(),
      correlationId,
      purchase.Id,
      purchase.BusinessId.Value,
      purchase.BranchId.Value,
      purchase.SupplierId,
      purchase.UserId,
      ToEventItems(purchase),
      purchase.Total,
      purchase.ReceivedAt ?? purchase.UpdatedAt);

  private static PurchaseItemV1[] ToEventItems(Purchase purchase)
    => purchase.Items
      .Select(item => new PurchaseItemV1(item.ProductId, item.Quantity, item.UnitCost, item.Subtotal))
      .ToArray();

  private Guid ResolveCorrelationId()
    => Guid.TryParse(correlationIdProvider.CorrelationId, out var parsedValue)
      ? parsedValue
      : Guid.NewGuid();

  private sealed record PurchaseUserContext(Guid BusinessId, Guid BranchId, Guid UserId);
}
