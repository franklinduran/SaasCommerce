using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.Modules.Sales.Domain;
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

    var businessId = new BusinessId(BusinessIdValue);
    var branchId = new BranchId(BranchIdValue);

    var hasProducts = await dbContext.Set<Product>().AnyAsync(ct);
    if (!hasProducts)
    {

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
    } // end if (!hasProducts)

    // Shared RNG for all seeding sections (deterministic seed → same data every fresh run).
    var rng = new Random(20240101);

    // ── 6. Historical sales (last 30 days) ────────────────────────────────────
    // Independent guard: runs even if products already existed (e.g. second startup).

    // Only skip if historical Completed sales already exist — ignores stray Processing/test sales.
    var hasCompletedSales = await dbContext.Set<Sale>()
      .AnyAsync(s => s.BusinessId == businessId && s.Status == SaleStatus.Completed, ct);
    if (!hasCompletedSales)
    {

    // Load products from DB (may have just been seeded above, or already existed).
    var productList = await dbContext.Set<Product>()
      .Where(p => p.BusinessId == businessId)
      .OrderBy(p => p.CreatedAt)
      .Take(20)
      .ToListAsync(ct);

    if (productList.Count == 0)
    {
      return;
    }

    // Prices and costs aligned to products[0..19] order above.
    decimal[] salePrices =
    [
      65m, 95m, 35m, 65m, 175m,
      45m, 185m, 55m, 75m, 120m,
      45m, 95m, 135m, 85m, 65m,
      145m, 130m, 175m, 65m, 125m,
    ];

    decimal[] costPrices =
    [
      47m, 68m, 20m, 45m, 140m,
      32m, 155m, 40m, 55m, 92m,
      32m, 75m, 105m, 65m, 48m,
      112m, 98m, 130m, 48m, 98m,
    ];

    // Payment method distribution: 60% Efectivo, 25% Tarjeta, 15% Transferencia.
    string[] paymentMethods = ["Efectivo", "Efectivo", "Efectivo", "Efectivo", "Efectivo", "Efectivo",
                               "Tarjeta", "Tarjeta", "Tarjeta",
                               "Transferencia", "Transferencia", "Transferencia",
                               "Efectivo", "Efectivo", "Tarjeta",
                               "Efectivo", "Efectivo", "Efectivo", "Tarjeta", "Transferencia"];

    var allSales = new List<Sale>(capacity: 30 * 8);

    for (var dayOffset = 29; dayOffset >= 0; dayOffset--)
    {
      var saleDay = now.AddDays(-dayOffset).Date;
      var dayOfWeek = saleDay.DayOfWeek;
      var isToday = dayOffset == 0;

      // Today always gets a good amount of sales so KPI cards are non-zero.
      var isWeekend = dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
      var minSales = isToday ? 8 : isWeekend ? 3 : 6;
      var maxSales = isToday ? 14 : isWeekend ? 6 : 12;
      var salesCount = rng.Next(minSales, maxSales + 1);

      for (var saleIndex = 0; saleIndex < salesCount; saleIndex++)
      {
        // For today: spread from 07:00 up to (now - 5 min) so CompletedAt is in the past.
        // For past days: spread across full business day 07:00-20:00.
        var maxHour = isToday ? Math.Max(0, now.Hour - 7) : 13;
        var hourOffset = maxHour > 0 ? rng.Next(0, maxHour) : 0;
        var minuteOffset = rng.Next(0, 60);
        var saleTime = new DateTimeOffset(
          saleDay.AddHours(7 + hourOffset).AddMinutes(minuteOffset),
          now.Offset);

        // 1–4 distinct products per sale.
        var lineCount = rng.Next(1, 5);
        var chosenIndexes = new HashSet<int>(capacity: lineCount);
        while (chosenIndexes.Count < lineCount)
        {
          chosenIndexes.Add(rng.Next(0, productList.Count));
        }

        var lines = chosenIndexes
          .Select(i => new SaleLine(
            ProductId: productList[i].Id,
            Quantity: rng.Next(1, 5),
            UnitPrice: salePrices[i],
            UnitCost: costPrices[i]))
          .ToList()
          .AsReadOnly();

        var payment = paymentMethods[rng.Next(0, paymentMethods.Length)];

        var sale = Sale.Create(
          id: Guid.NewGuid(),
          businessId: businessId,
          branchId: branchId,
          userId: adminUserId,
          lines: lines,
          paymentMethod: payment,
          createdAt: saleTime);

        sale.MarkAsProcessing(saleTime.AddSeconds(rng.Next(5, 60)));
        sale.Complete(saleTime.AddSeconds(rng.Next(60, 180)));

        allSales.Add(sale);
      }
    }

      await dbContext.Set<Sale>().AddRangeAsync(allSales, ct);
      await dbContext.SaveChangesAsync(ct);
    } // end if (!hasCompletedSales)

    // ── 6b. Cancelled sales (~10% of days) — enriches status breakdown chart ──

    var hasCancelledSales = await dbContext.Set<Sale>()
      .AnyAsync(s => s.BusinessId == businessId && s.Status == SaleStatus.Cancelled, ct);

    if (!hasCancelledSales)
    {
      var productList2 = await dbContext.Set<Product>()
        .Where(p => p.BusinessId == businessId)
        .OrderBy(p => p.CreatedAt)
        .Take(20)
        .ToListAsync(ct);

      decimal[] salePrices2 = [65m, 95m, 35m, 65m, 175m, 45m, 185m, 55m, 75m, 120m,
                                45m, 95m, 135m, 85m, 65m, 145m, 130m, 175m, 65m, 125m];
      decimal[] costPrices2  = [47m, 68m, 20m, 45m, 140m, 32m, 155m, 40m, 55m, 92m,
                                 32m, 75m, 105m, 65m, 48m, 112m, 98m, 130m, 48m, 98m];

      var rngC = new Random(20240201);
      string[] cancelReasons = ["Cliente cambió de opinión", "Error en el pedido", "Pago rechazado"];
      var cancelledSales = new List<Sale>();

      for (var dayOffset = 29; dayOffset >= 0; dayOffset--)
      {
        if (rngC.Next(0, 10) < 7) continue; // ~30% de días tienen cancelaciones

        var saleDay = now.AddDays(-dayOffset).Date;
        var count = rngC.Next(1, 3);

        for (var i = 0; i < count; i++)
        {
          var saleTime = new DateTimeOffset(
            saleDay.AddHours(8 + rngC.Next(0, 10)).AddMinutes(rngC.Next(0, 60)),
            now.Offset);

          var idx = rngC.Next(0, productList2.Count);
          var lines = new List<SaleLine>
          {
            new(productList2[idx].Id, rngC.Next(1, 3), salePrices2[idx], costPrices2[idx]),
          }.AsReadOnly();

          var sale = Sale.Create(
            id: Guid.NewGuid(),
            businessId: businessId,
            branchId: branchId,
            userId: adminUserId,
            lines: lines,
            paymentMethod: "Efectivo",
            createdAt: saleTime);

          sale.MarkAsProcessing(saleTime.AddSeconds(rngC.Next(5, 30)));
          sale.Cancel(cancelReasons[rngC.Next(0, cancelReasons.Length)], saleTime.AddSeconds(rngC.Next(30, 90)));
          cancelledSales.Add(sale);
        }
      }

      if (cancelledSales.Count > 0)
      {
        await dbContext.Set<Sale>().AddRangeAsync(cancelledSales, ct);
        await dbContext.SaveChangesAsync(ct);
      }
    }

    // ── 6c. Failed sales (~5% rate) — adds another slice to status chart ──────

    var hasFailedSales = await dbContext.Set<Sale>()
      .AnyAsync(s => s.BusinessId == businessId && s.Status == SaleStatus.Failed, ct);

    if (!hasFailedSales)
    {
      var productList3 = await dbContext.Set<Product>()
        .Where(p => p.BusinessId == businessId)
        .OrderBy(p => p.CreatedAt)
        .Take(10)
        .ToListAsync(ct);

      decimal[] salePrices3 = [65m, 95m, 35m, 65m, 175m, 45m, 185m, 55m, 75m, 120m];
      decimal[] costPrices3  = [47m, 68m, 20m, 45m, 140m, 32m, 155m, 40m, 55m, 92m];

      var rngF = new Random(20240301);
      var failedSales = new List<Sale>();

      for (var dayOffset = 29; dayOffset >= 0; dayOffset -= rngF.Next(3, 7))
      {
        var saleDay = now.AddDays(-dayOffset).Date;
        var saleTime = new DateTimeOffset(
          saleDay.AddHours(9 + rngF.Next(0, 8)).AddMinutes(rngF.Next(0, 60)),
          now.Offset);

        var idx = rngF.Next(0, productList3.Count);
        var lines = new List<SaleLine>
        {
          new(productList3[idx].Id, 1, salePrices3[idx], costPrices3[idx]),
        }.AsReadOnly();

        var sale = Sale.Create(
          id: Guid.NewGuid(),
          businessId: businessId,
          branchId: branchId,
          userId: adminUserId,
          lines: lines,
          paymentMethod: "Tarjeta",
          createdAt: saleTime);

        sale.MarkAsProcessing(saleTime.AddSeconds(rngF.Next(5, 30)));
        sale.Fail("Error en procesamiento de pago", saleTime.AddSeconds(rngF.Next(30, 120)));
        failedSales.Add(sale);
      }

      if (failedSales.Count > 0)
      {
        await dbContext.Set<Sale>().AddRangeAsync(failedSales, ct);
        await dbContext.SaveChangesAsync(ct);
      }
    }

    // ── 7. Invoices — issued for ~40% of completed sales ─────────────────────

    var hasInvoices = await dbContext.Set<Invoice>()
      .AnyAsync(i => i.BusinessId == businessId, ct);

    if (!hasInvoices)
    {
      var completedSales = await dbContext.Set<Sale>()
        .AsNoTracking()
        .Where(s => s.BusinessId == businessId && s.Status == SaleStatus.Completed)
        .OrderBy(s => s.CreatedAt)
        .ToListAsync(ct);

      var invoices = new List<Invoice>(capacity: completedSales.Count / 2);
      var seq = 1;

      foreach (var sale in completedSales)
      {
        // Issue invoice for roughly every other sale (40% coverage).
        if (rng.Next(0, 10) >= 4) continue;

        var inv = Invoice.Issue(
          id: Guid.NewGuid(),
          saleId: sale.Id,
          sequence: seq++,
          context: new InvoiceContext(businessId, branchId, null),
          financials: new InvoiceFinancials(
            Subtotal: sale.Total,
            DiscountTotal: 0m,
            TaxTotal: 0m,
            Total: sale.Total),
          createdAt: sale.CompletedAt ?? sale.CreatedAt);

        invoices.Add(inv);
      }

      await dbContext.Set<Invoice>().AddRangeAsync(invoices, ct);
      await dbContext.SaveChangesAsync(ct);
    }

    // ── 8. Credit accounts with pending balances ──────────────────────────────

    var hasCreditAccounts = await dbContext.Set<CustomerCreditAccount>()
      .AnyAsync(c => c.BusinessId == businessId && c.CurrentBalance > 0, ct);

    if (!hasCreditAccounts)
    {
      var customers = await dbContext.Set<Customer>()
        .Where(c => c.BusinessId == businessId)
        .Take(3)
        .ToListAsync(ct);

      var creditSales = await dbContext.Set<Sale>()
        .AsNoTracking()
        .Where(s => s.BusinessId == businessId && s.Status == SaleStatus.Completed)
        .OrderByDescending(s => s.CreatedAt)
        .Take(6)
        .ToListAsync(ct);

      var creditAccounts = new List<CustomerCreditAccount>(customers.Count);
      var creditMovements = new List<CustomerCreditMovement>(customers.Count * 2);

      decimal[] pendingAmounts = [1_850m, 3_200m, 750m];

      for (var ci = 0; ci < customers.Count && ci < creditSales.Count; ci++)
      {
        var account = new CustomerCreditAccount(
          id: Guid.NewGuid(),
          businessId: businessId,
          customerId: customers[ci].Id,
          creditLimit: 10_000m,
          createdAt: now.AddDays(-20));

        var saleForDebit = creditSales[ci];
        var movement = account.ApplyDebit(
          movementId: Guid.NewGuid(),
          saleId: saleForDebit.Id,
          amount: pendingAmounts[ci],
          note: "Crédito por compra",
          createdBy: adminUserId,
          createdAt: saleForDebit.CreatedAt.AddMinutes(1));

        creditAccounts.Add(account);
        creditMovements.Add(movement);
      }

      await dbContext.Set<CustomerCreditAccount>().AddRangeAsync(creditAccounts, ct);
      await dbContext.Set<CustomerCreditMovement>().AddRangeAsync(creditMovements, ct);
      await dbContext.SaveChangesAsync(ct);
    }

    // ── 9. Historical purchases (30 days, Completed) ──────────────────────────

    var hasCompletedPurchases = await dbContext.Set<Purchase>()
      .AnyAsync(p => p.BusinessId == businessId && p.Status == PurchaseStatus.Completed, ct);

    if (!hasCompletedPurchases)
    {
      var productList4 = await dbContext.Set<Product>()
        .Where(p => p.BusinessId == businessId)
        .OrderBy(p => p.CreatedAt)
        .Take(20)
        .ToListAsync(ct);

      var supplierList = await dbContext.Set<Supplier>()
        .Where(s => s.BusinessId == businessId)
        .ToListAsync(ct);

      if (productList4.Count > 0 && supplierList.Count > 0)
      {
        decimal[] costPrices4 = [47m, 68m, 20m, 45m, 140m, 32m, 155m, 40m, 55m, 92m,
                                   32m, 75m, 105m, 65m, 48m, 112m, 98m, 130m, 48m, 98m];

        var rngP = new Random(20240401);
        var allPurchases = new List<Purchase>();

        for (var dayOffset = 29; dayOffset >= 0; dayOffset--)
        {
          // 2-3 purchases per week (roughly every 2-3 days)
          if (rngP.Next(0, 7) >= 3) continue;

          var purchaseDay = now.AddDays(-dayOffset).Date;
          var purchaseTime = new DateTimeOffset(
            purchaseDay.AddHours(8 + rngP.Next(0, 4)).AddMinutes(rngP.Next(0, 60)),
            now.Offset);

          var supplier = supplierList[rngP.Next(0, supplierList.Count)];

          // 3-8 product lines per purchase order
          var lineCount = rngP.Next(3, 9);
          var chosenIdxs = new HashSet<int>(capacity: lineCount);
          while (chosenIdxs.Count < lineCount)
          {
            chosenIdxs.Add(rngP.Next(0, productList4.Count));
          }

          var purchaseLines = chosenIdxs
            .Select(i => new PurchaseLine(
              ProductId: productList4[i].Id,
              Quantity: rngP.Next(12, 48),
              UnitCost: costPrices4[i]))
            .ToList()
            .AsReadOnly();

          var purchase = Purchase.Create(
            new PurchaseCreationData(
              Id: Guid.NewGuid(),
              BusinessId: businessId,
              BranchId: branchId,
              SupplierId: supplier.Id,
              UserId: adminUserId,
              SupplierInvoiceNumber: $"FAC-{rngP.Next(1000, 9999)}",
              PurchaseDate: purchaseTime,
              Notes: null,
              CreatedAt: purchaseTime),
            purchaseLines);

          var receivedAt = purchaseTime.AddMinutes(rngP.Next(30, 120));
          purchase.Receive(receivedAt);
          purchase.StartProcessing(receivedAt.AddMinutes(rngP.Next(5, 20)));
          purchase.MarkInventoryUpdated(receivedAt.AddMinutes(rngP.Next(20, 40)));
          purchase.Complete(receivedAt.AddMinutes(rngP.Next(40, 90)));

          allPurchases.Add(purchase);
        }

        if (allPurchases.Count > 0)
        {
          await dbContext.Set<Purchase>().AddRangeAsync(allPurchases, ct);
          await dbContext.SaveChangesAsync(ct);
        }
      }
    }
  } // end SeedAsync

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
