using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Import;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Catalog.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;

namespace SaasCommerce.Modules.Tests;

public sealed class ImportProductsTests
{
  private static readonly DateTimeOffset FixedNow =
    new(2026, 5, 24, 12, 0, 0, TimeSpan.Zero);

  private static readonly Guid BusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
  private static readonly Guid BranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
  private static readonly Guid UserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

  [Fact]
  public async Task ImportProductsHandlerShouldCreateProductsWhenCsvIsValid()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Arroz El Gallo 5lbs,ARR-GALL-5LB,Víveres,175.00,140.00,100
      Cerveza Presidente 12oz,CRV-PRES-12,Bebidas,65.00,47.00,120
      """;

    var result = await handler.Handle(
      new ImportProductsCommand(ToStream(csv), CreateInitialInventory: true));

    result.IsSuccess.Should().BeTrue();
    result.Value.ImportedCount.Should().Be(2);
    result.Value.SkippedCount.Should().Be(0);

    var productCount = await db.Set<Product>()
      .Where(p => p.BusinessId == new SaasCommerce.SharedKernel.Tenancy.BusinessId(BusinessId))
      .CountAsync();
    productCount.Should().Be(2);
  }

  [Fact]
  public async Task ImportProductsHandlerShouldCreateInitialInventoryMovementsWhenStockIsProvided()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Leche Parmalat 1L,LEC-PARM-1L,Lácteos,95.00,75.00,60
      """;

    await handler.Handle(new ImportProductsCommand(ToStream(csv), CreateInitialInventory: true));

    var movementCount = await db.Set<InventoryMovement>()
      .Where(m => m.BusinessId == new SaasCommerce.SharedKernel.Tenancy.BusinessId(BusinessId)
        && m.Reason == InventoryMovementReason.InitialStock)
      .CountAsync();
    movementCount.Should().Be(1);

    var stockCount = await db.Set<StockItem>()
      .Where(s => s.BusinessId == new SaasCommerce.SharedKernel.Tenancy.BusinessId(BusinessId)
        && s.Quantity == 60m)
      .CountAsync();
    stockCount.Should().Be(1);
  }

  [Fact]
  public async Task ImportProductsHandlerShouldRejectInvalidRowsWhenRequiredFieldsAreMissing()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      ,MISSING-NAME,Bebidas,65.00,47.00,10
      Valid Product,VALID-SKU,,50.00,35.00,5
      """;

    var result = await handler.Handle(new ImportProductsCommand(ToStream(csv)));

    result.IsSuccess.Should().BeTrue();
    result.Value.ImportedCount.Should().Be(1);
    result.Value.SkippedCount.Should().Be(1);
    result.Value.Errors.Should().ContainSingle();
  }

  [Fact]
  public async Task ImportProductsHandlerShouldUseCurrentUserBusinessIdWhenImportingProducts()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Test Product,TEST-SKU-001,,100.00,70.00,20
      """;

    await handler.Handle(new ImportProductsCommand(ToStream(csv)));

    // Verify product is scoped to the authenticated user's BusinessId
    var product = await db.Set<Product>()
      .FirstOrDefaultAsync(p => p.Sku == "TEST-SKU-001");

    product.Should().NotBeNull();
    product!.BusinessId.Value.Should().Be(BusinessId);
  }

  [Fact]
  public async Task ImportProductsHandlerShouldNotDuplicateProductsWhenSkuAlreadyExists()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Arroz Gallo,ARR-DUP,Víveres,175.00,140.00,50
      """;

    await handler.Handle(new ImportProductsCommand(ToStream(csv)));
    var result2 = await handler.Handle(new ImportProductsCommand(ToStream(csv)));

    result2.IsSuccess.Should().BeTrue();
    result2.Value.SkippedCount.Should().Be(1);
    result2.Value.Errors.Should().ContainSingle(e =>
      e.Messages.Any(m => m.Contains("already exists")));
  }

  [Fact]
  public async Task ImportProductsHandlerShouldReturnErrorWhenFileIsEmpty()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var result = await handler.Handle(
      new ImportProductsCommand(ToStream(string.Empty)));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("IMPORT_EMPTY_FILE");
  }

  [Fact]
  public async Task ImportProductsHandlerShouldCreateCategoryWhenCategoryNameIsNew()
  {
    await using var db = CreateDbContext();
    var handler = CreateHandler(db);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Ron Barceló,RON-BARC-750,Licores Premium,1250.00,950.00,20
      """;

    await handler.Handle(new ImportProductsCommand(ToStream(csv)));

    var categoryExists = await db.Set<Category>()
      .AnyAsync(c => c.Name == "Licores Premium"
        && c.BusinessId == new SaasCommerce.SharedKernel.Tenancy.BusinessId(BusinessId));
    categoryExists.Should().BeTrue();
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static ImportProductsHandler CreateHandler(AppDbContext db)
  {
    var clock = new FixedClock(FixedNow);
    var currentUser = new FakeCurrentUser(BusinessId, BranchId, UserId);
    var productRepo = new EfCatalogProductRepository(db);
    var categoryRepo = new EfCatalogCategoryRepository(db);
    var inventoryRepo = new EfInventoryRepository(db);
    var unitOfWork = new EfUnitOfWork(db);

    return new ImportProductsHandler(
      productRepo,
      categoryRepo,
      inventoryRepo,
      currentUser,
      clock,
      unitOfWork);
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static MemoryStream ToStream(string content)
    => new MemoryStream(Encoding.UTF8.GetBytes(content));

  // ── Fakes ─────────────────────────────────────────────────────────────────

  private sealed class FakeCurrentUser(Guid businessId, Guid branchId, Guid userId)
    : ICurrentUserService
  {
    public Guid? UserId => userId;
    public Guid? BusinessId => businessId;
    public Guid? BranchId => branchId;
    public IReadOnlyCollection<string> Roles => ["Admin"];
    public bool IsAuthenticated => true;
  }

  private sealed class EfUnitOfWork(AppDbContext db)
    : SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => db.SaveChangesAsync(cancellationToken);
  }

  private sealed class FixedClock(DateTimeOffset utcNow) : IClock
  {
    public DateTimeOffset UtcNow => utcNow;
  }
}
