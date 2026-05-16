using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed class UpdateProductHandler(
  ICatalogProductRepository products,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<ProductResponse>> Handle(
    UpdateProductCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<ProductResponse>> HandleCoreAsync(
    UpdateProductCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.UserContextRequired);
    }

    if (!TryParse(command, out var productType, out var unitOfMeasure, out var taxCategory))
    {
      return Result.Failure<ProductResponse>(CatalogErrors.InvalidProduct);
    }

    var tenantId = new BusinessId(businessId);
    var product = await products.GetByIdAsync(command.ProductId, tenantId, cancellationToken);

    if (product is null)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.ProductNotFound);
    }

    var normalizedSku = command.Sku.Trim().ToUpperInvariant();
    var existingSku = await products.GetBySkuAsync(normalizedSku, tenantId, cancellationToken);

    if (existingSku is not null && existingSku.Id != product.Id)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.DuplicateSku);
    }

    if (!string.IsNullOrWhiteSpace(command.Barcode))
    {
      var existingBarcode = await products.GetByBarcodeAsync(
        command.Barcode.Trim(),
        tenantId,
        cancellationToken);

      if (existingBarcode is not null && existingBarcode.Id != product.Id)
      {
        return Result.Failure<ProductResponse>(CatalogErrors.DuplicateBarcode);
      }
    }

    try
    {
      product.Update(
        CreateIdentity(command, productType, unitOfMeasure),
        CreateCodes(command, normalizedSku),
        CreatePricing(command, taxCategory),
        CreateInventorySettings(command),
        CreateOptions(command),
        clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.InvalidProduct);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.InvalidProduct);
    }

    if (!command.IsActive)
    {
      product.Deactivate(clock.UtcNow);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(ProductResponseMapper.ToResponse(product));
  }

  private static bool TryParse(
    UpdateProductCommand command,
    out ProductType productType,
    out UnitOfMeasure unitOfMeasure,
    out TaxCategory taxCategory)
  {
    productType = default;
    unitOfMeasure = default;
    taxCategory = default;

    return Enum.TryParse(command.ProductType, true, out productType) &&
      Enum.TryParse(command.UnitOfMeasure, true, out unitOfMeasure) &&
      Enum.TryParse(command.TaxCategory, true, out taxCategory);
  }

  private static ProductIdentity CreateIdentity(
    UpdateProductCommand command,
    ProductType productType,
    UnitOfMeasure unitOfMeasure)
    => new(
      productType,
      command.Name,
      command.Description,
      command.CategoryId,
      command.BrandId,
      unitOfMeasure);

  private static ProductCodes CreateCodes(
    UpdateProductCommand command,
    string normalizedSku)
    => new(
      normalizedSku,
      command.Barcode,
      command.InternalCode,
      command.SupplierCode);

  private static ProductPricing CreatePricing(
    UpdateProductCommand command,
    TaxCategory taxCategory)
    => new(
      command.SalePrice,
      command.CostPrice,
      command.WholesalePrice,
      command.MinSalePrice,
      taxCategory,
      command.TaxRate,
      command.IsTaxIncluded);

  private static ProductInventorySettings CreateInventorySettings(UpdateProductCommand command)
    => new(
      command.TrackInventory,
      command.MinimumStock,
      command.MaximumStock,
      command.ReorderPoint,
      command.AllowNegativeStock);

  private static ProductOptions CreateOptions(UpdateProductCommand command)
    => new(
      command.AllowsDiscount,
      command.ParentProductId,
      command.VariantName,
      command.AttributesJson);
}
