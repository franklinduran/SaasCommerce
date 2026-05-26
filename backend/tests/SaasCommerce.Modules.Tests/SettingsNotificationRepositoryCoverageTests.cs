#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.Modules.Notifications.Infrastructure.Persistence;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.Modules.Settings.Infrastructure;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class SettingsNotificationRepositoryCoverageTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task SettingsRepositories_ShouldAddUpdateAndReadByBusiness()
  {
    await using var db = CreateDbContext();
    var businessId = BusinessId.New();
    var userId = Guid.NewGuid();

    var businessRepo = new EfBusinessSettingsRepository(db);
    var salesRepo = new EfSalesSettingsRepository(db);
    var inventoryRepo = new EfInventorySettingsRepository(db);
    var billingRepo = new EfBillingSettingsRepository(db);

    await businessRepo.UpsertAsync(BusinessSettings.Default(businessId, userId, Now));
    await salesRepo.UpsertAsync(SalesSettings.Default(businessId, userId, Now));
    await inventoryRepo.UpsertAsync(InventorySettings.Default(businessId, userId, Now));
    await billingRepo.UpsertAsync(BillingSettings.Default(businessId, userId, Now));
    await db.SaveChangesAsync();

    (await businessRepo.GetAsync(businessId))!.Currency.Should().Be("DOP");
    (await salesRepo.GetAsync(businessId))!.AllowDiscounts.Should().BeTrue();
    (await inventoryRepo.GetAsync(businessId))!.DefaultLowStockThreshold.Should().Be(5);
    (await billingRepo.GetAsync(businessId))!.InvoicePrefix.Should().Be("RI");

    await businessRepo.UpsertAsync(BusinessSettings.Default(businessId, userId, Now));
    await salesRepo.UpsertAsync(SalesSettings.Default(businessId, userId, Now));
    await inventoryRepo.UpsertAsync(InventorySettings.Default(businessId, userId, Now));
    await billingRepo.UpsertAsync(BillingSettings.Default(businessId, userId, Now));

    db.ChangeTracker.Clear();

    await businessRepo.UpsertAsync(new BusinessSettings(new BusinessSettingsDetails
    {
      BusinessId = businessId,
      CommercialName = "Updated Shop",
      Currency = "usd",
      Timezone = "UTC",
      UpdatedBy = userId,
      UpdatedAt = Now.AddHours(1)
    }));
    await salesRepo.UpsertAsync(new SalesSettings(new SalesSettingsDetails
    {
      BusinessId = businessId,
      AllowNegativeStock = true,
      AllowDiscounts = false,
      RequireCustomerForCreditSale = false,
      DefaultPaymentMethod = " Card ",
      EnableReceiptPrintAfterSale = true,
      EnableInvoiceAutoGeneration = false,
      UpdatedBy = userId,
      UpdatedAt = Now.AddHours(1)
    }));
    await inventoryRepo.UpsertAsync(new InventorySettings(businessId, false, 8, true, true, userId, Now.AddHours(1)));
    await billingRepo.UpsertAsync(new BillingSettings(new BillingSettingsDetails
    {
      BusinessId = businessId,
      ReceiptHeaderText = " Header ",
      ReceiptFooterText = " Footer ",
      ShowLogoOnReceipt = true,
      ShowRncOnReceipt = true,
      EnableInvoiceAutoGeneration = false,
      InvoicePrefix = " b2b ",
      InvoiceSequenceStart = 12,
      UpdatedBy = userId,
      UpdatedAt = Now.AddHours(1)
    }));
    await db.SaveChangesAsync();

    (await businessRepo.GetAsync(businessId))!.CommercialName.Should().Be("Updated Shop");
    (await salesRepo.GetAsync(businessId))!.DefaultPaymentMethod.Should().Be("Card");
    (await inventoryRepo.GetAsync(businessId))!.DefaultLowStockThreshold.Should().Be(8);
    (await billingRepo.GetAsync(businessId))!.InvoicePrefix.Should().Be("B2B");
  }

  [Fact]
  public async Task OperationalNotificationRepository_ShouldFilterPageAndCountUnreadNotifications()
  {
    await using var db = CreateDbContext();
    var businessId = BusinessId.New();
    var otherBusinessId = BusinessId.New();
    var branchId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var repo = new EfOperationalNotificationRepository(db);

    var first = Notification(businessId, branchId, OperationalNotificationType.LowStock, Now.AddMinutes(-10));
    var second = Notification(businessId, branchId, OperationalNotificationType.LowStock, Now);
    var read = Notification(businessId, Guid.NewGuid(), OperationalNotificationType.InvoiceFailed, Now.AddMinutes(-5));
    read.MarkRead(userId, Now.AddMinutes(1));

    await repo.AddAsync(first);
    await repo.AddAsync(second);
    await repo.AddAsync(read);
    await repo.AddAsync(Notification(otherBusinessId, branchId, OperationalNotificationType.LowStock, Now.AddMinutes(5)));
    await repo.SaveChangesAsync();

    var fetched = await repo.GetByIdAsync(first.Id, businessId);
    var wrongTenant = await repo.GetByIdAsync(first.Id, otherBusinessId);
    var page = await repo.GetPagedAsync(
      businessId,
      branchId,
      OperationalNotificationStatus.Unread,
      OperationalNotificationType.LowStock,
      page: 1,
      pageSize: 1);
    var unreadCount = await repo.GetUnreadCountAsync(businessId);

    fetched.Should().NotBeNull();
    wrongTenant.Should().BeNull();
    page.TotalCount.Should().Be(2);
    page.Items.Should().ContainSingle();
    page.Items[0].Id.Should().Be(second.Id);
    unreadCount.Should().Be(2);
  }

  private static OperationalNotification Notification(
    BusinessId businessId,
    Guid? branchId,
    OperationalNotificationType type,
    DateTimeOffset createdAt)
    => OperationalNotification.Create(new OperationalNotificationDraft
    {
      Id = Guid.NewGuid(),
      BusinessId = businessId,
      BranchId = branchId,
      Type = type,
      Severity = OperationalNotificationSeverity.Warning,
      Title = "Alert",
      Message = "Check this",
      CreatedAt = createdAt
    });

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }
}

#pragma warning restore CA1707
