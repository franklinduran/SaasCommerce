using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class GetPurchasesHandler(
  IPurchaseRepository purchases,
  ISupplierRepository suppliers,
  IProductPurchaseReader products,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50];

  public Task<Result<PurchaseListResponse>> Handle(
    GetPurchasesQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<PurchaseListResponse>> HandleCoreAsync(
    GetPurchasesQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<PurchaseListResponse>(PurchaseErrors.UserContextRequired);
    }

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize) ||
        !TryParseSort(query.SortBy, query.SortDirection, out var sortBy, out var sortDirection))
    {
      return Result.Failure<PurchaseListResponse>(PurchaseErrors.InvalidPurchase);
    }

    var tenantId = new BusinessId(businessId);
    var criteria = new PurchaseSearchCriteria(
      query.SupplierId,
      query.BranchId,
      query.Status,
      query.Query,
      query.DateFrom,
      query.DateTo,
      query.Page,
      query.PageSize,
      sortBy,
      sortDirection);
    var totalItems = await purchases.CountAsync(tenantId, criteria, cancellationToken);
    var totalPurchased = await purchases.SumTotalAsync(tenantId, criteria, cancellationToken);
    var purchaseItems = await purchases.ListAsync(tenantId, criteria, cancellationToken);
    var suppliersById = await suppliers.ListByIdsAsync(
      tenantId,
      purchaseItems.Select(purchase => purchase.SupplierId).Distinct().ToArray(),
      cancellationToken);
    var productMap = await products.ListAsync(
      businessId,
      purchaseItems.SelectMany(purchase => purchase.Items).Select(item => item.ProductId).Distinct().ToArray(),
      cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

    return Result.Success(new PurchaseListResponse(
      purchaseItems
        .Select(purchase => PurchaseResponseMapper.ToResponse(
          purchase,
          suppliersById.TryGetValue(purchase.SupplierId, out var supplier) ? supplier.Name : null,
          productMap))
        .ToArray(),
      query.Page,
      query.PageSize,
      totalItems,
      totalPages,
      query.Page > 1,
      totalPages > 0 && query.Page < totalPages,
      totalPurchased));
  }

  private static bool TryParseSort(
    string? sortByValue,
    string? sortDirectionValue,
    out PurchaseSortOption sortBy,
    out PurchaseSortDirection sortDirection)
  {
    sortBy = PurchaseSortOption.PurchaseDate;
    sortDirection = PurchaseSortDirection.Desc;

    if (!string.IsNullOrWhiteSpace(sortByValue) &&
        !Enum.TryParse(sortByValue, true, out sortBy))
    {
      return false;
    }

    return string.IsNullOrWhiteSpace(sortDirectionValue) ||
      Enum.TryParse(sortDirectionValue, true, out sortDirection);
  }
}
