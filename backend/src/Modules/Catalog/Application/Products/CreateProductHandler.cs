using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Products;

public sealed class CreateProductHandler(
  ICatalogProductRepository products,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<ProductResponse>> Handle(
    CreateProductCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.UserContextRequired);
    }

    if (!TryParse(command, out var productType, out var unitOfMeasure, out var taxCategory))
    {
      return Result.Failure<ProductResponse>(CatalogErrors.InvalidProduct);
    }

    var tenantId = new BusinessId(businessId);
    var normalizedSku = command.Sku.Trim().ToUpperInvariant();
    var existingSku = await products.GetBySkuAsync(normalizedSku, tenantId, cancellationToken);

    if (existingSku is not null)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.DuplicateSku);
    }

    if (!string.IsNullOrWhiteSpace(command.Barcode))
    {
      var existingBarcode = await products.GetByBarcodeAsync(
        command.Barcode.Trim(),
        tenantId,
        cancellationToken);

      if (existingBarcode is not null)
      {
        return Result.Failure<ProductResponse>(CatalogErrors.DuplicateBarcode);
      }
    }

    Product product;

    try
    {
      product = new Product(
        Guid.NewGuid(),
        tenantId,
        productType,
        command.Name,
        command.Description,
        normalizedSku,
        command.Barcode,
        command.CategoryId,
        command.BrandId,
        unitOfMeasure,
        command.SalePrice,
        command.CostPrice,
        command.WholesalePrice,
        command.MinSalePrice,
        taxCategory,
        command.TaxRate,
        command.IsTaxIncluded,
        command.AllowsDiscount,
        command.TrackInventory,
        command.MinimumStock,
        command.MaximumStock,
        command.ReorderPoint,
        command.AllowNegativeStock,
        command.InternalCode,
        command.SupplierCode,
        command.ParentProductId,
        command.VariantName,
        command.AttributesJson,
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

    await products.AddAsync(product, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(ProductResponseMapper.ToResponse(product));
  }

  private static bool TryParse(
    CreateProductCommand command,
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
}
