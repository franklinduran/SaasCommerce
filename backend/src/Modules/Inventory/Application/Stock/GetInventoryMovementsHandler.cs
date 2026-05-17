using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed class GetInventoryMovementsHandler(
  IInventoryRepository inventory,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<InventoryMovementListResponse>> Handle(
    GetInventoryMovementsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<InventoryMovementListResponse>> HandleCoreAsync(
    GetInventoryMovementsQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.BranchId is not Guid branchId)
    {
      return Result.Failure<InventoryMovementListResponse>(InventoryErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var currentBranchId = new BranchId(branchId);
    var page = Math.Max(1, query.Page);
    var pageSize = NormalizePageSize(query.PageSize);
    var criteria = new InventoryMovementSearchCriteria(
      query.ProductId,
      ParseMovementType(query.MovementType),
      query.DateFrom,
      query.DateTo,
      page,
      pageSize,
      ParseMovementSort(query.SortBy),
      ParseSortDirection(query.SortDirection, InventorySortDirection.Desc));
    var total = await inventory.CountMovementsAsync(
      tenantId,
      currentBranchId,
      criteria,
      cancellationToken);
    var items = await inventory.ListMovementsAsync(
      tenantId,
      currentBranchId,
      criteria,
      cancellationToken);
    var totalPages = CalculateTotalPages(total, pageSize);

    return Result.Success(new InventoryMovementListResponse(
      items.Select(InventoryResponseMapper.ToResponse).ToArray(),
      page,
      pageSize,
      total,
      total,
      totalPages,
      page > 1,
      page < totalPages));
  }

  private static int NormalizePageSize(int pageSize)
    => AllowedPageSizes.Contains(pageSize) ? pageSize : 10;

  private static int CalculateTotalPages(int totalItems, int pageSize)
    => totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

  private static InventoryMovementReason? ParseMovementType(string? movementType)
  {
    if (Enum.TryParse<InventoryMovementReason>(movementType, true, out var parsed))
    {
      return parsed;
    }

    return movementType?.Trim().ToLowerInvariant() switch
    {
      "initialload" => InventoryMovementReason.InitialStock,
      "purchase" => InventoryMovementReason.PurchaseEntry,
      "sale" => InventoryMovementReason.SaleDeduction,
      "adjustment" or "manualcorrection" => InventoryMovementReason.ManualAdjustment,
      _ => null
    };
  }

  private static InventoryMovementSortOption ParseMovementSort(string? sortBy)
  {
    if (string.Equals(sortBy, "productId", StringComparison.OrdinalIgnoreCase))
    {
      return InventoryMovementSortOption.ProductId;
    }

    if (string.Equals(sortBy, "quantity", StringComparison.OrdinalIgnoreCase))
    {
      return InventoryMovementSortOption.Quantity;
    }

    return InventoryMovementSortOption.CreatedAt;
  }

  private static InventorySortDirection ParseSortDirection(
    string? sortDirection,
    InventorySortDirection defaultDirection)
  {
    if (string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase))
    {
      return InventorySortDirection.Asc;
    }

    if (string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
    {
      return InventorySortDirection.Desc;
    }

    return defaultDirection;
  }
}
