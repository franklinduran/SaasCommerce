using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Development;

/// <summary>
/// Seeds realistic Dominican colmado/minimarket demo data for development and demonstrations.
/// Creates categories, products, customers, suppliers, and initial inventory.
/// Idempotent — safe to call multiple times.
/// </summary>
public static class DemoDataSeeder
{
  private static readonly Guid BusinessIdValue = Guid.Parse("11111111-1111-1111-1111-111111111111");
  private static readonly Guid BranchIdValue = Guid.Parse("22222222-2222-2222-2222-222222222222");

  public static async Task SeedAsync(
    AppDbContext dbContext,
    Guid adminUserId,
    DateTimeOffset now,
    CancellationToken ct = default)
  {
    ArgumentNullException.ThrowIfNull(dbContext);

    var hasProducts = await dbContext.Set<Product>().AnyAsync(ct);
    if (hasProducts)
    {
      return;
    }

    var businessId = new BusinessId(BusinessIdValue);
    var branchId = new BranchId(BranchIdValue);

    // ── 1. Categories ─────────────────────────────────────────────────────────

    var catBebidas = new Category(Guid.NewGuid(), businessId, "Bebidas", "Refrescos, cervezas, aguas y jugos", now);
    var catViveres = new Category(Guid.NewGuid(), businessId, "Víveres", "Arroz, habichuelas, aceite y abarrotes básicos", now);
    var catLacteos = new Category(Guid.NewGuid(), businessId, "Lácteos", "Leche, queso, mantequilla y derivados", now);
    var catCarnes = new Category(Guid.NewGuid(), businessId, "Carnes y Embutidos", "Pollo, salami, salchichas y embutidos", now);
    var catLimpieza = new Category(Guid.NewGuid(), businessId, "Limpieza y Hogar", "Productos de limpieza y cuidado del hogar", now);

    var categories = new[] { catBebidas, catViveres, catLacteos, catCarnes, catLimpieza };

    // ── 2. Products ───────────────────────────────────────────────────────────

    var products = new List<Product>
    {
      // Bebidas
      MakeProduct((businessId, "Cerveza Presidente 12oz", "CRV-PRES-12", catBebidas.Id,
        65m, 47m, 58m, 24m, 48m, now)),
      MakeProduct((businessId, "Coca-Cola 2 Litros", "BCO-COLA-2L", catBebidas.Id,
        95m, 68m, 85m, 12m, 24m, now)),
      MakeProduct((businessId, "Agua Cristal 1.5L", "AGU-CRIST-15", catBebidas.Id,
        35m, 20m, 30m, 24m, 48m, now)),
      MakeProduct((businessId, "Jugo Tropical Naranja 1L", "JUG-TROP-NAR", catBebidas.Id,
        65m, 45m, 58m, 12m, 24m, now)),

      // Víveres
      MakeProduct((businessId, "Arroz El Gallo 5lbs", "ARR-GALL-5LB", catViveres.Id,
        175m, 140m, 160m, 20m, 40m, now)),
      MakeProduct((businessId, "Habichuelas Rojas La Cibaeña 1lb", "HAB-CIBA-1LB", catViveres.Id,
        45m, 32m, 40m, 30m, 60m, now)),
      MakeProduct((businessId, "Aceite Iberia 1 Litro", "ACE-IBER-1L", catViveres.Id,
        185m, 155m, 170m, 12m, 24m, now)),
      MakeProduct((businessId, "Azúcar Morena Carrizal 2lbs", "AZU-CARR-2LB", catViveres.Id,
        55m, 40m, 50m, 20m, 40m, now)),
      MakeProduct((businessId, "Café Santo Domingo 4oz", "CAF-SDOM-4OZ", catViveres.Id,
        75m, 55m, 68m, 15m, 30m, now)),
      MakeProduct((businessId, "Salami Induveca 225g", "SAL-INDU-225", catViveres.Id,
        120m, 92m, 110m, 15m, 30m, now)),
      MakeProduct((businessId, "Espagueti Vitarrico 400g", "ESP-VITA-400", catViveres.Id,
        45m, 32m, 40m, 20m, 40m, now)),

      // Lácteos
      MakeProduct((businessId, "Leche Parmalat Entera 1L", "LEC-PARM-1L", catLacteos.Id,
        95m, 75m, 87m, 12m, 24m, now)),
      MakeProduct((businessId, "Queso Americano Rica 400g", "QUE-RICA-400", catLacteos.Id,
        135m, 105m, 125m, 10m, 20m, now)),
      MakeProduct((businessId, "Mantequilla Bonlé 100g", "MAN-BONL-100", catLacteos.Id,
        85m, 65m, 78m, 10m, 20m, now)),

      // Carnes y Embutidos
      MakeWeighedProduct((businessId, "Pollo Entero (por libra)", "POL-ENT-LB", catCarnes.Id,
        65m, 48m, 20m, 40m, now)),
      MakeProduct((businessId, "Salchichas Induveca 450g", "SALC-INDU-450", catCarnes.Id,
        145m, 112m, 133m, 12m, 24m, now)),
      MakeProduct((businessId, "Longaniza 1lb", "LON-INDU-1LB", catCarnes.Id,
        130m, 98m, 118m, 10m, 20m, now)),

      // Limpieza y Hogar
      MakeProduct((businessId, "Cloro Supreme 1 Galón", "CLO-SUP-1GL", catLimpieza.Id,
        175m, 130m, 160m, 10m, 20m, now)),
      MakeProduct((businessId, "Jabón Rey 400g", "JAB-REY-400", catLimpieza.Id,
        65m, 48m, 58m, 15m, 30m, now)),
      MakeProduct((businessId, "Suavitel Concentrado 1L", "SUA-CONC-1L", catLimpieza.Id,
        125m, 98m, 115m, 10m, 20m, now)),
    };

    // ── 3. Customers ──────────────────────────────────────────────────────────

    var customers = new[]
    {
      new Customer(Guid.NewGuid(), businessId, "María Altagracia Rodríguez", "8094561234", null, now),
      new Customer(Guid.NewGuid(), businessId, "Juan Carlos Pérez Sánchez", "8492345678", null, now),
      new Customer(Guid.NewGuid(), businessId, "Ana Belkis Martínez", "8097894321", null, now),
      new Customer(Guid.NewGuid(), businessId, "Pedro Rafael González", "8295678901", null, now),
      new Customer(Guid.NewGuid(), businessId, "Carmen Yolanda Díaz", "8093456789", null, now),
    };

    // ── 4. Suppliers ──────────────────────────────────────────────────────────

    var suppliers = new[]
    {
      new Supplier(Guid.NewGuid(), businessId, "Distribuidora Rojas & Asociados SRL",
        new SupplierContactInfo("132001231", "8092345678", "distribuidora.rojas@gmail.com", "Santiago, RD"), now),
      new Supplier(Guid.NewGuid(), businessId, "Importadora Central del Norte SRL",
        new SupplierContactInfo("101456789", "8095678901", null, "Santo Domingo Norte, RD"), now),
      new Supplier(Guid.NewGuid(), businessId, "Proveedor General de Víveres SA",
        new SupplierContactInfo("132678903", "8294567890", null, "La Vega, RD"), now),
    };

    await dbContext.Set<Category>().AddRangeAsync(categories, ct);
    await dbContext.Set<Product>().AddRangeAsync(products, ct);
    await dbContext.Set<Customer>().AddRangeAsync(customers, ct);
    await dbContext.Set<Supplier>().AddRangeAsync(suppliers, ct);
    await dbContext.SaveChangesAsync(ct);

    // ── 5. Initial inventory ──────────────────────────────────────────────────

    var stockData = new (Product Product, decimal Qty)[]
    {
      (products[0],  120m),  // Cerveza Presidente
      (products[1],   80m),  // Coca-Cola 2L
      (products[2],  200m),  // Agua Cristal
      (products[3],   60m),  // Jugo Tropical
      (products[4],  150m),  // Arroz El Gallo
      (products[5],  200m),  // Habichuelas Rojas
      (products[6],   90m),  // Aceite Iberia
      (products[7],  180m),  // Azúcar Morena
      (products[8],  100m),  // Café Santo Domingo
      (products[9],   80m),  // Salami Induveca
      (products[10], 120m),  // Espagueti Vitarrico
      (products[11], 100m),  // Leche Parmalat
      (products[12],  50m),  // Queso Americano
      (products[13],  60m),  // Mantequilla Bonlé
      (products[14],  80m),  // Pollo Entero (lbs)
      (products[15],  70m),  // Salchichas Induveca
      (products[16],  50m),  // Longaniza
      (products[17],  40m),  // Cloro Supreme
      (products[18], 150m),  // Jabón Rey
      (products[19],  60m),  // Suavitel
    };

    var stockItems = new List<StockItem>(stockData.Length);
    var movements = new List<InventoryMovement>(stockData.Length);

    foreach (var (product, qty) in stockData)
    {
      var item = new StockItem(Guid.NewGuid(), businessId, branchId, product.Id, now);
      var movement = item.ApplyAdjustment(qty, InventoryMovementReason.InitialStock, adminUserId, false, now);
      stockItems.Add(item);
      movements.Add(movement);
    }

    await dbContext.Set<StockItem>().AddRangeAsync(stockItems, ct);
    await dbContext.Set<InventoryMovement>().AddRangeAsync(movements, ct);
    await dbContext.SaveChangesAsync(ct);
  }

  // ── Private helpers ─────────────────────────────────────────────────────────

  private static Product MakeProduct(
    (BusinessId BusinessId,
      string Name,
      string Sku,
      Guid CategoryId,
      decimal SalePrice,
      decimal CostPrice,
      decimal? Wholesale,
      decimal? MinStock,
      decimal? Reorder,
      DateTimeOffset Now) seed) => new(
      new ProductCreationContext(Guid.NewGuid(), seed.BusinessId, seed.Now),
      new ProductIdentity(ProductType.Simple, seed.Name, null, seed.CategoryId, null, UnitOfMeasure.Unit),
      new ProductCodes(seed.Sku, null, null, null),
      new ProductPricing(seed.SalePrice, seed.CostPrice, seed.Wholesale, null, TaxCategory.Exempt, 0m, false),
      new ProductInventorySettings(true, seed.MinStock, null, seed.Reorder, false),
      new ProductOptions(true, null, null, null));

  private static Product MakeWeighedProduct(
    (BusinessId BusinessId,
      string Name,
      string Sku,
      Guid CategoryId,
      decimal SalePrice,
      decimal CostPrice,
      decimal? MinStock,
      decimal? Reorder,
      DateTimeOffset Now) seed) => new(
      new ProductCreationContext(Guid.NewGuid(), seed.BusinessId, seed.Now),
      new ProductIdentity(ProductType.Weighed, seed.Name, null, seed.CategoryId, null, UnitOfMeasure.Pound),
      new ProductCodes(seed.Sku, null, null, null),
      new ProductPricing(seed.SalePrice, seed.CostPrice, null, null, TaxCategory.Exempt, 0m, false),
      new ProductInventorySettings(true, seed.MinStock, null, seed.Reorder, false),
      new ProductOptions(true, null, null, null));
}
