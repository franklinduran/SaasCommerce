using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Domain;

public sealed class Product
{
  private Product()
  {
  }

  public Product(
    Guid id,
    BusinessId businessId,
    ProductType productType,
    string name,
    string? description,
    string sku,
    string? barcode,
    Guid? categoryId,
    Guid? brandId,
    UnitOfMeasure unitOfMeasure,
    decimal salePrice,
    decimal costPrice,
    decimal? wholesalePrice,
    decimal? minSalePrice,
    TaxCategory taxCategory,
    decimal taxRate,
    bool isTaxIncluded,
    bool allowsDiscount,
    bool trackInventory,
    decimal? minimumStock,
    decimal? maximumStock,
    decimal? reorderPoint,
    bool allowNegativeStock,
    string? internalCode,
    string? supplierCode,
    Guid? parentProductId,
    string? variantName,
    string? attributesJson,
    DateTimeOffset createdAt)
  {
    Validate(
      productType,
      name,
      sku,
      unitOfMeasure,
      salePrice,
      costPrice,
      wholesalePrice,
      minSalePrice,
      trackInventory);

    Id = id;
    BusinessId = businessId;
    ProductType = productType;
    Name = name.Trim();
    Description = NormalizeOptional(description);
    Sku = sku.Trim().ToUpperInvariant();
    Barcode = NormalizeOptional(barcode);
    CategoryId = categoryId;
    BrandId = brandId;
    UnitOfMeasure = unitOfMeasure;
    SalePrice = salePrice;
    CostPrice = costPrice;
    WholesalePrice = wholesalePrice;
    MinSalePrice = minSalePrice;
    TaxCategory = taxCategory;
    TaxRate = taxRate;
    IsTaxIncluded = isTaxIncluded;
    AllowsDiscount = allowsDiscount;
    TrackInventory = trackInventory;
    MinimumStock = minimumStock;
    MaximumStock = maximumStock;
    ReorderPoint = reorderPoint;
    AllowNegativeStock = allowNegativeStock;
    InternalCode = NormalizeOptional(internalCode);
    SupplierCode = NormalizeOptional(supplierCode);
    ParentProductId = parentProductId;
    VariantName = NormalizeOptional(variantName);
    AttributesJson = NormalizeOptional(attributesJson);
    SearchName = NormalizeSearchName(name);
    CreatedAt = createdAt;
    IsActive = true;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public ProductType ProductType { get; private set; }

  public Guid? CategoryId { get; private set; }

  public Guid? BrandId { get; private set; }

  public Guid? ParentProductId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public string? Description { get; private set; }

  public string Sku { get; private set; } = string.Empty;

  public string? Barcode { get; private set; }

  public string SearchName { get; private set; } = string.Empty;

  public string? InternalCode { get; private set; }

  public string? SupplierCode { get; private set; }

  public UnitOfMeasure UnitOfMeasure { get; private set; }

  public decimal SalePrice { get; private set; }

  public decimal CostPrice { get; private set; }

  public decimal? WholesalePrice { get; private set; }

  public decimal? MinSalePrice { get; private set; }

  public TaxCategory TaxCategory { get; private set; }

  public decimal TaxRate { get; private set; }

  public bool IsTaxIncluded { get; private set; }

  public bool AllowsDiscount { get; private set; }

  public bool TrackInventory { get; private set; }

  public decimal? MinimumStock { get; private set; }

  public decimal? MaximumStock { get; private set; }

  public decimal? ReorderPoint { get; private set; }

  public bool AllowNegativeStock { get; private set; }

  public string? VariantName { get; private set; }

  public string? AttributesJson { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public decimal? ProfitMargin
    => SalePrice == 0 ? null : Math.Round(((SalePrice - CostPrice) / SalePrice) * 100, 2);

  public void Update(
    ProductType productType,
    string name,
    string? description,
    string sku,
    string? barcode,
    Guid? categoryId,
    Guid? brandId,
    UnitOfMeasure unitOfMeasure,
    decimal salePrice,
    decimal costPrice,
    decimal? wholesalePrice,
    decimal? minSalePrice,
    TaxCategory taxCategory,
    decimal taxRate,
    bool isTaxIncluded,
    bool allowsDiscount,
    bool trackInventory,
    decimal? minimumStock,
    decimal? maximumStock,
    decimal? reorderPoint,
    bool allowNegativeStock,
    string? internalCode,
    string? supplierCode,
    Guid? parentProductId,
    string? variantName,
    string? attributesJson,
    DateTimeOffset updatedAt)
  {
    Validate(
      productType,
      name,
      sku,
      unitOfMeasure,
      salePrice,
      costPrice,
      wholesalePrice,
      minSalePrice,
      trackInventory);

    ProductType = productType;
    Name = name.Trim();
    Description = NormalizeOptional(description);
    Sku = sku.Trim().ToUpperInvariant();
    Barcode = NormalizeOptional(barcode);
    CategoryId = categoryId;
    BrandId = brandId;
    UnitOfMeasure = unitOfMeasure;
    SalePrice = salePrice;
    CostPrice = costPrice;
    WholesalePrice = wholesalePrice;
    MinSalePrice = minSalePrice;
    TaxCategory = taxCategory;
    TaxRate = taxRate;
    IsTaxIncluded = isTaxIncluded;
    AllowsDiscount = allowsDiscount;
    TrackInventory = trackInventory;
    MinimumStock = minimumStock;
    MaximumStock = maximumStock;
    ReorderPoint = reorderPoint;
    AllowNegativeStock = allowNegativeStock;
    InternalCode = NormalizeOptional(internalCode);
    SupplierCode = NormalizeOptional(supplierCode);
    ParentProductId = parentProductId;
    VariantName = NormalizeOptional(variantName);
    AttributesJson = NormalizeOptional(attributesJson);
    SearchName = NormalizeSearchName(name);
    UpdatedAt = updatedAt;
  }

  public void Deactivate(DateTimeOffset updatedAt)
  {
    IsActive = false;
    UpdatedAt = updatedAt;
  }

  private static void Validate(
    ProductType productType,
    string name,
    string sku,
    UnitOfMeasure unitOfMeasure,
    decimal salePrice,
    decimal costPrice,
    decimal? wholesalePrice,
    decimal? minSalePrice,
    bool trackInventory)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(sku);

    if (salePrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(salePrice), "Sale price cannot be negative.");
    }

    if (costPrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(costPrice), "Cost price cannot be negative.");
    }

    if (wholesalePrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wholesalePrice), "Wholesale price cannot be negative.");
    }

    if (minSalePrice < 0 || minSalePrice > salePrice)
    {
      throw new ArgumentOutOfRangeException(nameof(minSalePrice), "Minimum sale price must be between zero and sale price.");
    }

    if (productType == ProductType.Service && trackInventory)
    {
      throw new InvalidOperationException("Services cannot track inventory.");
    }

    if (productType == ProductType.Weighed && unitOfMeasure == UnitOfMeasure.Unit)
    {
      throw new InvalidOperationException("Weighed products cannot use Unit as unit of measure.");
    }

    if (trackInventory && unitOfMeasure == UnitOfMeasure.Service)
    {
      throw new InvalidOperationException("Inventory products require a physical unit of measure.");
    }
  }

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

  private static string NormalizeSearchName(string name)
    => name.Trim().ToUpperInvariant();
}
