using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Categories;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Catalog.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Infrastructure.Inventory;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Infrastructure.Availability;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;

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
      10,
      "name",
      "asc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.Page.Should().Be(2);
    result.Value.PageSize.Should().Be(10);
    result.Value.TotalItems.Should().Be(12);
    result.Value.TotalPages.Should().Be(2);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task CreateCategoryShouldPersistCategoryForCurrentBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateCategoryHandler(dbContext, currentUser);

    var result = await handler.Handle(new CreateCategoryCommand("Bebidas", "Productos liquidos"));

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(currentUser.BusinessId!.Value);
    result.Value.Name.Should().Be("Bebidas");
    result.Value.Description.Should().Be("Productos liquidos");
    result.Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task CreateCategoryShouldRejectDuplicateNameInsideSameBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateCategoryHandler(dbContext, currentUser);

    await handler.Handle(new CreateCategoryCommand("Bebidas", null));
    var duplicate = await handler.Handle(new CreateCategoryCommand("Bebidas", null));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(CatalogErrors.DuplicateCategory);
  }

  [Fact]
  public async Task UpdateCategoryShouldUpdateOnlyCurrentBusinessCategory()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateCategoryHandler(dbContext, currentUser);
    var updateHandler = UpdateCategoryHandler(dbContext, currentUser);

    var created = await createHandler.Handle(new CreateCategoryCommand("Bebidas", null));
    var result = await updateHandler.Handle(new UpdateCategoryCommand(
      created.Value.Id,
      "Bebidas frias",
      "Nevera",
      false));

    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be("Bebidas frias");
    result.Value.Description.Should().Be("Nevera");
    result.Value.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task GetCategoriesShouldReturnOnlyCurrentBusinessCategories()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherUser = TestCurrentUser.Create();

    await CreateCategoryHandler(dbContext, currentUser)
      .Handle(new CreateCategoryCommand("Bebidas", null));
    await CreateCategoryHandler(dbContext, otherUser)
      .Handle(new CreateCategoryCommand("Ferreteria", null));

    var result = await new GetCategoriesHandler(
        new EfCatalogCategoryRepository(dbContext),
        currentUser)
      .Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle();
    result.Value.Single().Name.Should().Be("Bebidas");
  }

  [Fact]
  public async Task GetProductsShouldRejectUnsupportedPageSize()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = new GetProductsHandler(new EfCatalogProductRepository(dbContext), currentUser);

    var result = await handler.Handle(new GetProductsQuery(null, null, null, true, 1, 20, "name", "asc"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.InvalidProduct);
  }

  [Fact]
  public async Task GetProductsShouldSortBySalePriceDescending()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var listHandler = new GetProductsHandler(new EfCatalogProductRepository(dbContext), currentUser);

    await createHandler.Handle(CreateProductCommand("Producto barato", "SKU-LOW", salePrice: 100));
    await createHandler.Handle(CreateProductCommand("Producto caro", "SKU-HIGH", salePrice: 300));

    var result = await listHandler.Handle(new GetProductsQuery(null, null, null, true, 1, 10, "salePrice", "desc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.First().Name.Should().Be("Producto caro");
  }

  [Fact]
  public async Task DeactivateProductShouldSetProductInactive()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var created = await createHandler.Handle(CreateProductCommand("Cafe", "SKU-001"));
    var handler = new DeactivateProductHandler(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

    var result = await handler.Handle(new DeactivateProductCommand(created.Value.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task ActivateProductShouldSetProductActive()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var created = await createHandler.Handle(CreateProductCommand("Cafe", "SKU-001"));
    var repository = new EfCatalogProductRepository(dbContext);
    await new DeactivateProductHandler(repository, currentUser, new FixedClock(), new EfUnitOfWork(dbContext))
      .Handle(new DeactivateProductCommand(created.Value.Id));

    var result = await new ActivateProductHandler(repository, currentUser, new FixedClock(), new EfUnitOfWork(dbContext))
      .Handle(new ActivateProductCommand(created.Value.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task DeactivateProductShouldNotUpdateOtherTenantProduct()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherUser = TestCurrentUser.Create();
    var created = await CreateProductHandler(dbContext, otherUser)
      .Handle(CreateProductCommand("Cafe", "SKU-001"));
    var handler = new DeactivateProductHandler(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

    var result = await handler.Handle(new DeactivateProductCommand(created.Value.Id));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.ProductNotFound);
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
    result.Value.StockItem.BranchId.Should().Be(currentUser.BranchId!.Value);
    result.Value.StockItem.Quantity.Should().Be(12);
    result.Value.Movement.PreviousStock.Should().Be(0);
    result.Value.Movement.NewStock.Should().Be(12);
    result.Value.Movement.BranchId.Should().Be(currentUser.BranchId!.Value);
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

  [Fact]
  public async Task GetStockShouldFilterByCurrentBranch()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherBranchUser = currentUser with { BranchId = Guid.NewGuid() };
    var firstProductId = Guid.NewGuid();
    var secondProductId = Guid.NewGuid();

    await CreateInventoryHandler(
        dbContext,
        currentUser,
        ProductInventoryPolicy(firstProductId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(firstProductId, 5, "InitialLoad"));
    await CreateInventoryHandler(
        dbContext,
        otherBranchUser,
        ProductInventoryPolicy(secondProductId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(secondProductId, 8, "InitialLoad"));

    var result = await new GetStockHandler(
        new EfInventoryRepository(dbContext),
        new TestProductInventoryPolicyReader(ProductInventoryPolicy(firstProductId, currentUser.BusinessId!.Value)),
        currentUser)
      .Handle(new GetStockQuery(null, false, null, null, 1, 10, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Single().ProductId.Should().Be(firstProductId);
  }

  [Fact]
  public async Task GetStockShouldFilterLowStockOnly()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var productPolicy = ProductInventoryPolicy(productId, currentUser.BusinessId!.Value);

    await CreateInventoryHandler(dbContext, currentUser, productPolicy)
      .Handle(new AdjustInventoryCommand(productId, 5, "InitialLoad"));

    var result = await new GetStockHandler(
        new EfInventoryRepository(dbContext),
        new TestProductInventoryPolicyReader(productPolicy, minimumStock: 10),
        currentUser)
      .Handle(new GetStockQuery(null, true, null, null, 1, 10, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Single().IsLowStock.Should().BeTrue();
  }

  [Fact]
  public async Task GetInventoryMovementsShouldFilterByReason()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var productPolicy = ProductInventoryPolicy(productId, currentUser.BusinessId!.Value);
    var handler = CreateInventoryHandler(dbContext, currentUser, productPolicy);

    await handler.Handle(new AdjustInventoryCommand(productId, 5, "InitialLoad"));
    await handler.Handle(new AdjustInventoryCommand(productId, 8, "Purchase"));

    var result = await new GetInventoryMovementsHandler(new EfInventoryRepository(dbContext), currentUser)
      .Handle(new GetInventoryMovementsQuery(null, "Purchase", null, null, 1, 10, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Single().Reason.Should().Be("Purchase");
  }

  [Fact]
  public async Task ProductSalesPolicyShouldBlockInactiveProducts()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var created = await CreateProductHandler(dbContext, currentUser)
      .Handle(CreateProductCommand("Cafe", "SKU-001"));
    await new DeactivateProductHandler(
        new EfCatalogProductRepository(dbContext),
        currentUser,
        new FixedClock(),
        new EfUnitOfWork(dbContext))
      .Handle(new DeactivateProductCommand(created.Value.Id));

    var policy = await new EfProductInventoryPolicyReader(dbContext)
      .GetSalesPolicyAsync(currentUser.BusinessId!.Value, created.Value.Id);

    policy.Should().NotBeNull();
    policy!.CanBeSold.Should().BeFalse();
    policy.ReasonIfCannotBeSold.Should().Be("Product is inactive.");
  }

  [Fact]
  public async Task ProductSalesPolicyShouldAllowActiveServicesWithoutInventory()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var created = await CreateProductHandler(dbContext, currentUser)
      .Handle(CreateProductCommand(
        "Delivery",
        "SRV-001",
        productType: "Service",
        unitOfMeasure: "Service",
        trackInventory: false,
        salePrice: 100));

    var policy = await new EfProductInventoryPolicyReader(dbContext)
      .GetSalesPolicyAsync(currentUser.BusinessId!.Value, created.Value.Id);

    policy.Should().NotBeNull();
    policy!.CanBeSold.Should().BeTrue();
    policy.TrackInventory.Should().BeFalse();
    policy.ProductType.Should().Be("Service");
  }

  [Fact]
  public async Task InventoryAvailabilityShouldRejectInsufficientStockWhenNegativeStockIsNotAllowed()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    await CreateInventoryHandler(
        dbContext,
        currentUser,
        ProductInventoryPolicy(productId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(productId, 3, "InitialLoad"));

    var result = await new EfInventoryAvailabilityService(new EfInventoryRepository(dbContext))
      .ValidateStockAsync(new InventoryAvailabilityRequest(
        currentUser.BusinessId.Value,
        currentUser.BranchId!.Value,
        productId,
        5,
        true,
        false));

    result.IsAvailable.Should().BeFalse();
    result.AvailableQuantity.Should().Be(3);
    result.ReasonIfUnavailable.Should().Be("Insufficient stock.");
  }

  [Fact]
  public async Task InventoryAvailabilityShouldAllowNonInventoryProducts()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();

    var result = await new EfInventoryAvailabilityService(new EfInventoryRepository(dbContext))
      .ValidateStockAsync(new InventoryAvailabilityRequest(
        currentUser.BusinessId!.Value,
        currentUser.BranchId!.Value,
        Guid.NewGuid(),
        100,
        false,
        false));

    result.IsAvailable.Should().BeTrue();
    result.AvailableQuantity.Should().Be(0);
  }

  private static CreateProductHandler CreateProductHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static CreateCategoryHandler CreateCategoryHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogCategoryRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static UpdateCategoryHandler UpdateCategoryHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogCategoryRepository(dbContext),
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
    bool trackInventory = true,
    decimal salePrice = 250)
    => new(
      productType,
      name,
      "Producto de prueba",
      sku,
      barcode,
      null,
      null,
      unitOfMeasure,
      salePrice,
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

  private sealed class TestProductInventoryPolicyReader(
    ProductInventoryPolicy productPolicy,
    decimal? minimumStock = 1)
    : IProductInventoryPolicyReader,
      IInventoryProductLookupReader
  {
    public Task<ProductInventoryPolicy?> GetAsync(
      Guid businessId,
      Guid productId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        productPolicy.BusinessId == businessId && productPolicy.ProductId == productId
          ? productPolicy
          : null);

    public Task<IReadOnlyCollection<InventoryProductLookup>> SearchAsync(
      InventoryProductLookupQuery query,
      CancellationToken cancellationToken = default)
    {
      IReadOnlyCollection<InventoryProductLookup> products =
        productPolicy.BusinessId == query.BusinessId
          ? [new InventoryProductLookup(
              productPolicy.ProductId,
              productPolicy.BusinessId,
              "Producto de prueba",
              "SKU-TEST",
              null,
              productPolicy.ProductType,
              null,
              productPolicy.UnitOfMeasure,
              minimumStock,
              5)]
          : [];

      return Task.FromResult(products);
    }
  }

  private sealed record TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }

    public Guid? BusinessId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; }

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
