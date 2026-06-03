using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Domain;

public sealed class Product
{
  private Product()
  {
  }

  public Product(
    ProductCreationContext context,
    ProductIdentity identity,
    ProductCodes codes,
    ProductPricing pricing,
    ProductInventorySettings inventory,
    ProductOptions options)
  {
    Validate(identity, codes, pricing, inventory);

    Id = context.Id;
    BusinessId = context.BusinessId;
    ApplyDetails(identity, codes, pricing, inventory, options);
    CreatedAt = context.CreatedAt;
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

  public string? ImageUrl { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public decimal? ProfitMargin
    => SalePrice == 0 ? null : Math.Round(((SalePrice - CostPrice) / SalePrice) * 100, 2);

  public void Update(
    ProductIdentity identity,
    ProductCodes codes,
    ProductPricing pricing,
    ProductInventorySettings inventory,
    ProductOptions options,
    DateTimeOffset updatedAt)
  {
    Validate(identity, codes, pricing, inventory);
    ApplyDetails(identity, codes, pricing, inventory, options);
    UpdatedAt = updatedAt;
  }

  public void SetImageUrl(string? imageUrl, DateTimeOffset updatedAt)
  {
    ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    UpdatedAt = updatedAt;
  }

  public void Deactivate(DateTimeOffset updatedAt)
  {
    IsActive = false;
    UpdatedAt = updatedAt;
  }

  public void Activate(DateTimeOffset updatedAt)
  {
    IsActive = true;
    UpdatedAt = updatedAt;
  }

  public void UpdateAverageCost(
    decimal currentStock,
    decimal purchasedQuantity,
    decimal unitCost,
    DateTimeOffset updatedAt)
  {
    if (currentStock < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentStock), "Current stock cannot be negative.");
    }

    if (purchasedQuantity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(purchasedQuantity), "Purchased quantity must be greater than zero.");
    }

    if (unitCost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");
    }

    var totalQuantity = currentStock + purchasedQuantity;
    CostPrice = totalQuantity == 0
      ? unitCost
      : Math.Round(((currentStock * CostPrice) + (purchasedQuantity * unitCost)) / totalQuantity, 2);
    UpdatedAt = updatedAt;
  }

  private void ApplyDetails(
    ProductIdentity identity,
    ProductCodes codes,
    ProductPricing pricing,
    ProductInventorySettings inventory,
    ProductOptions options)
  {
    ProductType = identity.ProductType;
    Name = identity.Name.Trim();
    Description = NormalizeOptional(identity.Description);
    Sku = codes.Sku.Trim().ToUpperInvariant();
    Barcode = NormalizeOptional(codes.Barcode);
    CategoryId = identity.CategoryId;
    BrandId = identity.BrandId;
    UnitOfMeasure = identity.UnitOfMeasure;
    SalePrice = pricing.SalePrice;
    CostPrice = pricing.CostPrice;
    WholesalePrice = pricing.WholesalePrice;
    MinSalePrice = pricing.MinSalePrice;
    TaxCategory = pricing.TaxCategory;
    TaxRate = pricing.TaxRate;
    IsTaxIncluded = pricing.IsTaxIncluded;
    AllowsDiscount = options.AllowsDiscount;
    TrackInventory = inventory.TrackInventory;
    MinimumStock = inventory.MinimumStock;
    MaximumStock = inventory.MaximumStock;
    ReorderPoint = inventory.ReorderPoint;
    AllowNegativeStock = inventory.AllowNegativeStock;
    InternalCode = NormalizeOptional(codes.InternalCode);
    SupplierCode = NormalizeOptional(codes.SupplierCode);
    ParentProductId = options.ParentProductId;
    VariantName = NormalizeOptional(options.VariantName);
    AttributesJson = NormalizeOptional(options.AttributesJson);
    SearchName = NormalizeSearchName(identity.Name);
  }

  private static void Validate(
    ProductIdentity identity,
    ProductCodes codes,
    ProductPricing pricing,
    ProductInventorySettings inventory)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(identity.Name);
    ArgumentException.ThrowIfNullOrWhiteSpace(codes.Sku);

    if (pricing.SalePrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pricing), "Sale price cannot be negative.");
    }

    if (pricing.CostPrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pricing), "Cost price cannot be negative.");
    }

    if (pricing.WholesalePrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pricing), "Wholesale price cannot be negative.");
    }

    if (pricing.MinSalePrice < 0 || pricing.MinSalePrice > pricing.SalePrice)
    {
      throw new ArgumentOutOfRangeException(nameof(pricing), "Minimum sale price must be between zero and sale price.");
    }

    if (identity.ProductType == ProductType.Service && inventory.TrackInventory)
    {
      throw new InvalidOperationException("Services cannot track inventory.");
    }

    if (identity.ProductType == ProductType.Weighed && identity.UnitOfMeasure == UnitOfMeasure.Unit)
    {
      throw new InvalidOperationException("Weighed products cannot use Unit as unit of measure.");
    }

    if (inventory.TrackInventory && identity.UnitOfMeasure == UnitOfMeasure.Service)
    {
      throw new InvalidOperationException("Inventory products require a physical unit of measure.");
    }
  }

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

  private static string NormalizeSearchName(string name)
    => name.Trim().ToUpperInvariant();
}

public sealed record ProductCreationContext(
  Guid Id,
  BusinessId BusinessId,
  DateTimeOffset CreatedAt);

public sealed record ProductIdentity(
  ProductType ProductType,
  string Name,
  string? Description,
  Guid? CategoryId,
  Guid? BrandId,
  UnitOfMeasure UnitOfMeasure);

public sealed record ProductCodes(
  string Sku,
  string? Barcode,
  string? InternalCode,
  string? SupplierCode);

public sealed record ProductPricing(
  decimal SalePrice,
  decimal CostPrice,
  decimal? WholesalePrice,
  decimal? MinSalePrice,
  TaxCategory TaxCategory,
  decimal TaxRate,
  bool IsTaxIncluded);

public sealed record ProductInventorySettings(
  bool TrackInventory,
  decimal? MinimumStock,
  decimal? MaximumStock,
  decimal? ReorderPoint,
  bool AllowNegativeStock);

public sealed record ProductOptions(
  bool AllowsDiscount,
  Guid? ParentProductId,
  string? VariantName,
  string? AttributesJson);
