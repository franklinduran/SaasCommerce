#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Billing.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Catalog.Infrastructure.Inventory;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Customers.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class EfRepositoriesTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  // ── EfUserManagementRepository ───────────────────────────────────────────

  [Fact]
  public async Task UserManagement_GetUsers_ShouldReturnUsersOrderedByName()
  {
    await using var db = CreateDbContext();
    var bid = BusinessId.New();
    SeedUser(db, bid, "Zoraida", "z@test.com", SystemRoles.Cashier);
    SeedUser(db, bid, "Ana", "a@test.com", SystemRoles.Admin);
    await db.SaveChangesAsync();
    var repo = new EfUserManagementRepository(db);

    var result = await repo.GetUsersAsync(bid);

    result.TotalItems.Should().Be(2);
    result.Items.First().FullName.Should().Be("Ana");
  }

  [Fact]
  public async Task UserManagement_GetById_ShouldReturnUser_WhenExists()
  {
    await using var db = CreateDbContext();
    var bid = BusinessId.New();
    var user = SeedUser(db, bid, "Pedro", "p@test.com", SystemRoles.Admin);
    await db.SaveChangesAsync();
    var repo = new EfUserManagementRepository(db);

    var found = await repo.GetByIdInBusinessAsync(user.Id, bid);

    found.Should().NotBeNull();
    found!.FullName.Should().Be("Pedro");
  }

  [Fact]
  public async Task UserManagement_GetById_ShouldReturnNull_WhenWrongBusiness()
  {
    await using var db = CreateDbContext();
    var bid = BusinessId.New();
    var user = SeedUser(db, bid, "Pedro", "p@test.com", SystemRoles.Admin);
    await db.SaveChangesAsync();
    var repo = new EfUserManagementRepository(db);

    var found = await repo.GetByIdInBusinessAsync(user.Id, BusinessId.New());

    found.Should().BeNull();
  }

  [Fact]
  public async Task UserManagement_HasOtherAdminOrOwner_ShouldReturnTrue_WhenAnotherAdminExists()
  {
    await using var db = CreateDbContext();
    var bid = BusinessId.New();
    var target = SeedUser(db, bid, "Target", "t@test.com", SystemRoles.Owner);
    SeedUser(db, bid, "Other", "o@test.com", SystemRoles.Admin);
    await db.SaveChangesAsync();
    var repo = new EfUserManagementRepository(db);

    var result = await repo.HasOtherAdminOrOwnerAsync(target.Id, bid);

    result.Should().BeTrue();
  }

  [Fact]
  public async Task UserManagement_HasOtherAdminOrOwner_ShouldReturnFalse_WhenNoOther()
  {
    await using var db = CreateDbContext();
    var bid = BusinessId.New();
    var target = SeedUser(db, bid, "Target", "t@test.com", SystemRoles.Owner);
    await db.SaveChangesAsync();
    var repo = new EfUserManagementRepository(db);

    var result = await repo.HasOtherAdminOrOwnerAsync(target.Id, bid);

    result.Should().BeFalse();
  }

  // ── EfInvoiceRepository ──────────────────────────────────────────────────

  [Fact]
  public async Task Invoice_GetNextSequence_ShouldReturnOne_WhenEmpty()
  {
    await using var db = CreateDbContext();
    var repo = new EfInvoiceRepository(db);

    var next = await repo.GetNextSequenceAsync(new BusinessId(Guid.NewGuid()));

    next.Should().Be(1);
  }

  [Fact]
  public async Task Invoice_AddGetBySaleAndSequence_ShouldWork()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var saleId = Guid.NewGuid();
    var repo = new EfInvoiceRepository(db);
    var invoice = Invoice.Issue(
      Guid.NewGuid(), saleId, 1,
      new InvoiceContext(bid, new BranchId(Guid.NewGuid()), Guid.NewGuid()),
      new InvoiceFinancials(100, 0, 18, 118), Now);
    await repo.AddAsync(invoice);
    await db.SaveChangesAsync();

    (await repo.GetAsync(bid, invoice.Id)).Should().NotBeNull();
    (await repo.GetBySaleAsync(bid, saleId)).Should().NotBeNull();
    (await repo.GetNextSequenceAsync(bid)).Should().Be(2);
  }

  [Fact]
  public async Task Invoice_CountAndList_ShouldApplyAllFilters()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var repo = new EfInvoiceRepository(db);
    var invoice = Invoice.Issue(
      Guid.NewGuid(), Guid.NewGuid(), 1,
      new InvoiceContext(bid, new BranchId(Guid.NewGuid()), Guid.NewGuid()),
      new InvoiceFinancials(100, 0, 18, 118), Now);
    await repo.AddAsync(invoice);
    await db.SaveChangesAsync();
    var criteria = new InvoiceSearchCriteria(
      "Issued", invoice.InvoiceNumber, Now.AddDays(-1), Now.AddDays(1), 1, 10);

    (await repo.CountAsync(bid, criteria)).Should().Be(1);
    (await repo.ListAsync(bid, criteria)).Should().ContainSingle();
  }

  // ── EfCustomerCreditRepository ───────────────────────────────────────────

  [Fact]
  public async Task CustomerCredit_AddAndGetAccount_ShouldWork()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var customerId = Guid.NewGuid();
    var repo = new EfCustomerCreditRepository(db);
    await repo.AddAccountAsync(new CustomerCreditAccount(Guid.NewGuid(), bid, customerId, 1000, Now));
    await db.SaveChangesAsync();

    var account = await repo.GetAccountAsync(bid, customerId);

    account.Should().NotBeNull();
    account!.CreditLimit.Should().Be(1000);
  }

  [Fact]
  public async Task CustomerCredit_ListAccounts_ShouldReturnEmpty_WhenNoIds()
  {
    await using var db = CreateDbContext();
    var repo = new EfCustomerCreditRepository(db);

    var result = await repo.ListAccountsAsync(new BusinessId(Guid.NewGuid()), []);

    result.Should().BeEmpty();
  }

  [Fact]
  public async Task CustomerCredit_ListAccounts_ShouldReturnMatching()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var customerId = Guid.NewGuid();
    var repo = new EfCustomerCreditRepository(db);
    await repo.AddAccountAsync(new CustomerCreditAccount(Guid.NewGuid(), bid, customerId, 500, Now));
    await db.SaveChangesAsync();

    var result = await repo.ListAccountsAsync(bid, [customerId]);

    result.Should().ContainKey(customerId);
  }

  [Fact]
  public async Task CustomerCredit_MovementsAndPayments_ShouldPersistAndQuery()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var customerId = Guid.NewGuid();
    var saleId = Guid.NewGuid();
    var paymentId = Guid.NewGuid();
    var repo = new EfCustomerCreditRepository(db);
    var account = new CustomerCreditAccount(Guid.NewGuid(), bid, customerId, 5000, Now);
    var movement = account.ApplyDebit(Guid.NewGuid(), saleId, 200, "venta", null, Now);
    await repo.AddAccountAsync(account);
    await repo.AddMovementAsync(movement);
    await repo.AddPaymentAsync(new CustomerPayment(paymentId, bid, customerId, 50, null, Now, null));
    await db.SaveChangesAsync();

    (await repo.HasDebitForSaleAsync(bid, saleId)).Should().BeTrue();
    (await repo.HasPaymentAsync(bid, paymentId)).Should().BeTrue();
    (await repo.CountMovementsAsync(bid, customerId)).Should().Be(1);
    (await repo.ListMovementsAsync(bid, customerId, 1, 10)).Should().ContainSingle();
  }

  // ── EfProductInventoryPolicyReader ───────────────────────────────────────

  [Fact]
  public async Task PolicyReader_GetAsync_ShouldReturnPolicy_WhenProductExists()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    db.Add(MakeProduct(productId, bid, isActive: true, salePrice: 100));
    await db.SaveChangesAsync();
    var reader = new EfProductInventoryPolicyReader(db);

    var policy = await reader.GetAsync(bid.Value, productId);

    policy.Should().NotBeNull();
    policy!.TrackInventory.Should().BeTrue();
  }

  [Fact]
  public async Task PolicyReader_GetAsync_ShouldReturnNull_WhenMissing()
  {
    await using var db = CreateDbContext();
    var reader = new EfProductInventoryPolicyReader(db);

    var policy = await reader.GetAsync(Guid.NewGuid(), Guid.NewGuid());

    policy.Should().BeNull();
  }

  [Fact]
  public async Task PolicyReader_Search_ShouldApplyFilters()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    db.Add(MakeProduct(productId, bid, isActive: true, salePrice: 100));
    await db.SaveChangesAsync();
    var reader = new EfProductInventoryPolicyReader(db);

    var results = await reader.SearchAsync(
      new InventoryProductLookupQuery(bid.Value, "Producto", "Simple", null));

    results.Should().ContainSingle();
  }

  [Fact]
  public async Task PolicyReader_GetSalesPolicy_ShouldReturnPolicy_WhenActive()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    db.Add(MakeProduct(productId, bid, isActive: true, salePrice: 100));
    await db.SaveChangesAsync();
    var reader = new EfProductInventoryPolicyReader(db);

    var policy = await reader.GetSalesPolicyAsync(bid.Value, productId);

    policy.Should().NotBeNull();
    policy!.CanBeSold.Should().BeTrue();
  }

  [Fact]
  public async Task PolicyReader_GetSalesPolicy_ShouldBlock_WhenInactive()
  {
    await using var db = CreateDbContext();
    var bid = new BusinessId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    var product = MakeProduct(productId, bid, isActive: true, salePrice: 100);
    product.Deactivate(Now);
    db.Add(product);
    await db.SaveChangesAsync();
    var reader = new EfProductInventoryPolicyReader(db);

    var policy = await reader.GetSalesPolicyAsync(bid.Value, productId);

    policy.Should().NotBeNull();
    policy!.CanBeSold.Should().BeFalse();
    policy.ReasonIfCannotBeSold.Should().NotBeNull();
  }

  [Fact]
  public async Task PolicyReader_GetSalesPolicy_ShouldReturnNull_WhenMissing()
  {
    await using var db = CreateDbContext();
    var reader = new EfProductInventoryPolicyReader(db);

    var policy = await reader.GetSalesPolicyAsync(Guid.NewGuid(), Guid.NewGuid());

    policy.Should().BeNull();
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static User SeedUser(AppDbContext db, BusinessId bid, string name, string email, string roleName)
  {
    var role = new Role(Guid.NewGuid(), bid, roleName);
    var user = new User(Guid.NewGuid(), bid, null, name, email, "hash", Now);
    user.AddRole(role);
    db.Add(role);
    db.Add(user);
    return user;
  }

  private static Product MakeProduct(Guid id, BusinessId bid, bool isActive, decimal salePrice)
    => new(
      new ProductCreationContext(id, bid, Now),
      new ProductIdentity(ProductType.Simple, "Producto", null, null, null, UnitOfMeasure.Unit),
      new ProductCodes("SKU-001", null, null, null),
      new ProductPricing(salePrice, 50, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, 5, null, null, false),
      new ProductOptions(true, null, null, null));

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }
}

#pragma warning restore CA1707
