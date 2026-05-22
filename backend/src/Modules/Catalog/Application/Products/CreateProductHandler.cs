using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
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
  IUnitOfWork unitOfWork,
  ISubscriptionLimitChecker limitChecker)
{
  public Task<Result<ProductResponse>> Handle(
    CreateProductCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<ProductResponse>> HandleCoreAsync(
    CreateProductCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<ProductResponse>(CatalogErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);

    // Check subscription limits
    var limitCheck = await limitChecker.CanCreateProductAsync(tenantId, cancellationToken);
    if (!limitCheck.IsAllowed)
    {
      return Result.Failure<ProductResponse>(new DomainError("subscription.limit_reached", limitCheck.Message));
    }

    if (!TryParse(command, out var productType, out var unitOfMeasure, out var taxCategory))
    {
      return Result.Failure<ProductResponse>(CatalogErrors.InvalidProduct);
    }
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
        new ProductCreationContext(Guid.NewGuid(), tenantId, clock.UtcNow),
        CreateIdentity(command, productType, unitOfMeasure),
        CreateCodes(command, normalizedSku),
        CreatePricing(command, taxCategory),
        CreateInventorySettings(command),
        CreateOptions(command));
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

  private static ProductIdentity CreateIdentity(
    CreateProductCommand command,
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
    CreateProductCommand command,
    string normalizedSku)
    => new(
      normalizedSku,
      command.Barcode,
      command.InternalCode,
      command.SupplierCode);

  private static ProductPricing CreatePricing(
    CreateProductCommand command,
    TaxCategory taxCategory)
    => new(
      command.SalePrice,
      command.CostPrice,
      command.WholesalePrice,
      command.MinSalePrice,
      taxCategory,
      command.TaxRate,
      command.IsTaxIncluded);

  private static ProductInventorySettings CreateInventorySettings(CreateProductCommand command)
    => new(
      command.TrackInventory,
      command.MinimumStock,
      command.MaximumStock,
      command.ReorderPoint,
      command.AllowNegativeStock);

  private static ProductOptions CreateOptions(CreateProductCommand command)
    => new(
      command.AllowsDiscount,
      command.ParentProductId,
      command.VariantName,
      command.AttributesJson);
}
