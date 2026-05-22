using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class AdjustInventoryHandler(
  IInventoryRepository inventory,
  IProductInventoryPolicyReader productPolicies,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IAuditLogWriter auditLog,
  IClock clock,
  IUnitOfWork unitOfWork,
  ISubscriptionAccessPolicy subscriptionAccess)
{
  public Task<Result<InventoryAdjustmentResponse>> Handle(
    AdjustInventoryCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<InventoryAdjustmentResponse>> HandleCoreAsync(
    AdjustInventoryCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.UserContextRequired);
    }

    var branchId = command.BranchId ?? currentUser.BranchId;

    if (branchId is null)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.UserContextRequired);
    }

    if (command.ProductId == Guid.Empty ||
        command.Quantity == 0 ||
        !TryParseMovementReason(command.Reason, out var reason))
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.InvalidAdjustment);
    }

    var tenantId = new BusinessId(businessId);
    var access = await subscriptionAccess.EnsureCanUseFeatureAsync(
      tenantId,
      SubscriptionFeature.Products,
      cancellationToken);
    if (access.IsFailure)
    {
      return Result.Failure<InventoryAdjustmentResponse>(access.Error);
    }

    var currentBranchId = new BranchId(branchId.Value);
    var productPolicy = await productPolicies.GetAsync(businessId, command.ProductId, cancellationToken);

    if (productPolicy is null)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.ProductNotFound);
    }

    if (!productPolicy.TrackInventory)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.ProductDoesNotTrackInventory);
    }

    var stockItem = await inventory.GetStockItemAsync(
      tenantId,
      currentBranchId,
      command.ProductId,
      cancellationToken);
    var isNewStockItem = stockItem is null;

    stockItem ??= new StockItem(Guid.NewGuid(), tenantId, currentBranchId, command.ProductId, clock.UtcNow);

    InventoryMovement movement;

    try
    {
      movement = stockItem.ApplyAdjustment(
        command.Quantity,
        reason,
        userId,
        productPolicy.AllowNegativeStock,
        clock.UtcNow,
        new InventoryMovementSource(Note: command.Note));
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<InventoryAdjustmentResponse>(InventoryErrors.NegativeStock);
    }

    if (isNewStockItem)
    {
      await inventory.AddStockItemAsync(stockItem, cancellationToken);
    }

    await inventory.AddMovementAsync(movement, cancellationToken);
    await outbox.AddAsync(
      new InventoryAdjustedEventV1(
        Guid.NewGuid(),
        Guid.NewGuid(),
        businessId,
        branchId.Value,
        command.ProductId,
        movement.Id,
        movement.PreviousStock,
        movement.NewStock,
        clock.UtcNow),
      cancellationToken);

    if (stockItem.IsLowStock(productPolicy.MinimumStock) &&
        productPolicy.MinimumStock is decimal minimumStock)
    {
      await outbox.AddAsync(
        new LowStockDetectedEventV1(
          Guid.NewGuid(),
          Guid.NewGuid(),
          businessId,
          branchId.Value,
          command.ProductId,
          command.ProductId.ToString("D"),
          stockItem.Quantity,
          minimumStock,
          clock.UtcNow),
        cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    await auditLog.WriteAsync(
      new AuditEntry(
        tenantId,
        userId,
        "inventory.adjusted",
        "StockItem",
        stockItem.Id,
        $"Product {command.ProductId}: {movement.PreviousStock} → {movement.NewStock} ({command.Reason})"),
      cancellationToken);

    return Result.Success(new InventoryAdjustmentResponse(
      InventoryResponseMapper.ToResponse(stockItem, null),
      InventoryResponseMapper.ToResponse(movement)));
  }

  private static bool TryParseMovementReason(
    string? value,
    out InventoryMovementReason reason)
  {
    if (Enum.TryParse(value, true, out reason))
    {
      return true;
    }

    reason = value?.Trim().ToLowerInvariant() switch
    {
      "initialload" or "initialstock" or "initial_stock" => InventoryMovementReason.InitialStock,
      "sale" or "salededuction" or "sale_deduction" => InventoryMovementReason.SaleDeduction,
      "adjustment" or "manualcorrection" or "manualadjustment" or "manual_adjustment" => InventoryMovementReason.ManualAdjustment,
      "purchase" or "purchaseentry" or "purchase_entry" => InventoryMovementReason.PurchaseEntry,
      _ => default
    };

    return reason != default;
  }
}
