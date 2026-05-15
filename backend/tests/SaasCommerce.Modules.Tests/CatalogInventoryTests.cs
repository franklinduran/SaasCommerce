using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Tests;

public sealed class CatalogInventoryTests
{
  [Fact]
  public async Task CreateProductShouldPersistProductForCurrentBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    var result = await handler.Handle(CreateProductCommand("Cafe molido", "sku-001", "1234567890"));

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(currentUser.BusinessId!.Value);
    result.Value.Sku.Should().Be("SKU-001");
    result.Value.Barcode.Should().Be("1234567890");
    result.Value.SalePrice.Should().Be(250);
    result.Value.CostPrice.Should().Be(150);
    result.Value.ProfitMargin.Should().Be(40);
  }

  [Fact]
  public async Task CreateProductShouldRejectDuplicateSkuInsideSameBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    await handler.Handle(CreateProductCommand("Cafe", "SKU-001"));
    var duplicate = await handler.Handle(CreateProductCommand("Cafe premium", "sku-001"));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(CatalogErrors.DuplicateSku);
  }

  [Fact]
  public async Task CreateProductShouldRejectDuplicateBarcodeInsideSameBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    await handler.Handle(CreateProductCommand("Cafe", "SKU-001", "789"));
    var duplicate = await handler.Handle(CreateProductCommand("Cafe premium", "SKU-002", "789"));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(CatalogErrors.DuplicateBarcode);
  }

  [Fact]
  public async Task CreateProductShouldRejectServiceWithInventoryTracking()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    var result = await handler.Handle(CreateProductCommand(
      "Delivery",
      "SRV-001",
      productType: "Service",
      unitOfMeasure: "Service",
      trackInventory: true));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.InvalidProduct);
  }

  [Fact]
  public async Task CreateProductShouldRejectWeighedProductWithUnitMeasure()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    var result = await handler.Handle(CreateProductCommand(
      "Arroz suelto",
      "ARR-001",
      productType: "Weighed",
      unitOfMeasure: "Unit"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.InvalidProduct);
  }

  [Fact]
  public async Task GetProductsShouldReturnPagedResultWithTotalPages()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var listHandler = new GetProductsHandler(
      new EfCatalogProductRepository(dbContext),
      currentUser);

    for (var index = 1; index <= 12; index++)
    {
      await createHandler.Handle(CreateProductCommand($"Producto {index:00}", $"SKU-{index:00}"));
    }

    var result = await listHandler.Handle(new GetProductsQuery(
      "Producto",
      "Simple",
      null,
      true,
      2,
      10));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.Page.Should().Be(2);
    result.Value.PageSize.Should().Be(10);
    result.Value.TotalItems.Should().Be(12);
    result.Value.TotalPages.Should().Be(2);
  }

  [Fact]
  public async Task AdjustInventoryShouldCreateStockItemAndMovement()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var handler = CreateInventoryHandler(
      dbContext,
      currentUser,
      ProductInventoryPolicy(productId, currentUser.BusinessId!.Value));

    var result = await handler.Handle(new AdjustInventoryCommand(
      productId,
      12,
      "InitialLoad"));

    result.IsSuccess.Should().BeTrue();
    result.Value.StockItem.ProductId.Should().Be(productId);
    result.Value.StockItem.Quantity.Should().Be(12);
    result.Value.Movement.PreviousStock.Should().Be(0);
    result.Value.Movement.NewStock.Should().Be(12);
    result.Value.Movement.UserId.Should().Be(currentUser.UserId!.Value);
  }

  [Fact]
  public async Task AdjustInventoryShouldRejectNegativeResultingStock()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var handler = CreateInventoryHandler(
      dbContext,
      currentUser,
      ProductInventoryPolicy(productId, currentUser.BusinessId!.Value));

    var result = await handler.Handle(new AdjustInventoryCommand(
      productId,
      -1,
      "Adjustment"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InventoryErrors.NegativeStock);
  }

  [Fact]
  public async Task AdjustInventoryShouldRejectProductsWithoutInventoryTracking()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var handler = CreateInventoryHandler(
      dbContext,
      currentUser,
      ProductInventoryPolicy(
        productId,
        currentUser.BusinessId!.Value,
        productType: "Service",
        trackInventory: false,
        unitOfMeasure: "Service"));

    var result = await handler.Handle(new AdjustInventoryCommand(productId, 1, "Adjustment"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InventoryErrors.ProductDoesNotTrackInventory);
  }

  private static CreateProductHandler CreateProductHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static AdjustInventoryHandler CreateInventoryHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    ProductInventoryPolicy productPolicy)
    => new(
      new EfInventoryRepository(dbContext),
      new TestProductInventoryPolicyReader(productPolicy),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static CreateProductCommand CreateProductCommand(
    string name,
    string sku,
    string? barcode = null,
    string productType = "Simple",
    string unitOfMeasure = "Unit",
    bool trackInventory = true)
    => new(
      productType,
      name,
      "Producto de prueba",
      sku,
      barcode,
      null,
      null,
      unitOfMeasure,
      250,
      150,
      null,
      null,
      "Itbis18",
      18,
      true,
      true,
      trackInventory,
      1,
      100,
      5,
      false,
      null,
      null,
      null,
      null,
      null);

  private static ProductInventoryPolicy ProductInventoryPolicy(
    Guid productId,
    Guid businessId,
    string productType = "Simple",
    bool trackInventory = true,
    bool allowNegativeStock = false,
    string unitOfMeasure = "Unit")
    => new(
      productId,
      businessId,
      productType,
      trackInventory,
      allowNegativeStock,
      unitOfMeasure);

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow { get; } =
      new(2026, 5, 15, 12, 0, 0, TimeSpan.Zero);
  }

  private sealed class TestProductInventoryPolicyReader(ProductInventoryPolicy productPolicy)
    : IProductInventoryPolicyReader
  {
    public Task<ProductInventoryPolicy?> GetAsync(
      Guid businessId,
      Guid productId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        productPolicy.BusinessId == businessId && productPolicy.ProductId == productId
          ? productPolicy
          : null);
  }

  private sealed class TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; private init; }

    public Guid? BusinessId { get; private init; }

    public Guid? BranchId { get; private init; }

    public IReadOnlyCollection<string> Roles { get; private init; } = [];

    public bool IsAuthenticated { get; private init; }

    public static TestCurrentUser Create()
      => new()
      {
        UserId = Guid.NewGuid(),
        BusinessId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        Roles = ["Admin"],
        IsAuthenticated = true
      };
  }
}
