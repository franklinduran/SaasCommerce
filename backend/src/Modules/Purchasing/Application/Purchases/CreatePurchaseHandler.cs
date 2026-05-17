using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class CreatePurchaseHandler(
  IPurchaseRepository purchases,
  ISupplierRepository suppliers,
  IProductPurchaseReader products,
  PurchaseReceiptProcessor receiptProcessor,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  ICorrelationIdProvider correlationIdProvider,
  IClock clock,
  IUnitOfWork unitOfWork)
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
    var context = ResolveUserContext();

    if (context.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.UserContextRequired);
    }

    var userContext = context.Value;
    var branchId = command.BranchId ?? userContext.BranchId;

    if (!IsValidCommand(command) || branchId == Guid.Empty)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.InvalidPurchase);
    }

    var supplier = await suppliers.GetAsync(
      new BusinessId(userContext.BusinessId),
      command.SupplierId,
      cancellationToken);

    if (supplier is null || !supplier.IsActive)
    {
      return Result.Failure<PurchaseResponse>(PurchaseErrors.SupplierNotFound);
    }

    var lines = await BuildPurchaseLinesAsync(
      userContext.BusinessId,
      command.Items,
      cancellationToken);

    if (lines.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(lines.Error);
    }

    var purchase = CreatePurchase(command, userContext, branchId, lines.Value);

    if (purchase.IsFailure)
    {
      return Result.Failure<PurchaseResponse>(purchase.Error);
    }

    var entity = purchase.Value;
    var correlationId = ResolveCorrelationId();
    await purchases.AddAsync(entity, cancellationToken);
    await outbox.AddAsync(
      ToCreatedEvent(entity, correlationId),
      cancellationToken);

    if (command.ReceiveNow)
    {
      var receipt = await receiptProcessor.ProcessAsync(
        entity,
        userContext.UserId,
        correlationId,
        markAsReceived: true,
        cancellationToken);

      if (receipt.IsFailure)
      {
        return Result.Failure<PurchaseResponse>(receipt.Error);
      }

      await outbox.AddAsync(ToReceivedEvent(entity, correlationId), cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    var productMap = await products.ListAsync(
      userContext.BusinessId,
      entity.Items.Select(item => item.ProductId).Distinct().ToArray(),
      cancellationToken);

    return Result.Success(PurchaseResponseMapper.ToResponse(entity, supplier.Name, productMap));
  }

  private Result<PurchaseUserContext> ResolveUserContext()
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId ||
        currentUser.BranchId is not Guid branchId)
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

    var product = await products.GetAsync(businessId, item.ProductId, cancellationToken);

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
    PurchaseUserContext context,
    Guid branchId,
    IReadOnlyCollection<PurchaseLine> lines)
  {
    try
    {
      return Result.Success(Purchase.Create(
        Guid.NewGuid(),
        new BusinessId(context.BusinessId),
        new BranchId(branchId),
        command.SupplierId,
        context.UserId,
        lines,
        command.SupplierInvoiceNumber,
        command.PurchaseDate ?? clock.UtcNow,
        command.Notes,
        clock.UtcNow));
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
