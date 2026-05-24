using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class DailyClosingConfiguration : IEntityTypeConfiguration<DailyClosing>
{
  public void Configure(EntityTypeBuilder<DailyClosing> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("daily_closings", "sales");

    builder.HasKey(dc => dc.Id);

    builder.Property(dc => dc.Id)
      .ValueGeneratedNever();

    builder.Property(dc => dc.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(dc => dc.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(dc => dc.CreatedByUserId)
      .IsRequired();

    builder.Property(dc => dc.ClosingDate)
      .HasColumnType("date")
      .IsRequired();

    builder.Property(dc => dc.Status)
      .HasConversion<string>()
      .HasMaxLength(20)
      .IsRequired();

    // Sales
    builder.Property(dc => dc.TotalSales).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.CashSales).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.TransferSales).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.CardSales).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.CreditSales).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.SalesCount).IsRequired();

    // Cash
    builder.Property(dc => dc.CashExpected).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.CashCounted).HasPrecision(18, 2);
    builder.Property(dc => dc.CashDifference).HasPrecision(18, 2);

    // Expenses
    builder.Property(dc => dc.TotalExpenses).HasPrecision(18, 2).IsRequired();

    // Profitability
    builder.Property(dc => dc.TotalCost).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.GrossProfit).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.EstimatedNetProfit).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.GrossMarginPercent).HasPrecision(8, 2).IsRequired();
    builder.Property(dc => dc.NetMarginPercent).HasPrecision(8, 2).IsRequired();

    // Credits
    builder.Property(dc => dc.NewCreditsAmount).HasPrecision(18, 2).IsRequired();
    builder.Property(dc => dc.NewCreditsCount).IsRequired();
    builder.Property(dc => dc.CreditPaymentsReceived).HasPrecision(18, 2).IsRequired();

    // Notes / metadata
    builder.Property(dc => dc.Notes).HasMaxLength(2_000);
    builder.Property(dc => dc.CreatedAt).IsRequired();
    builder.Property(dc => dc.ClosedAt);
    builder.Property(dc => dc.ClosedByUserId);
    builder.Property(dc => dc.UpdatedAt).IsRequired();

    // Alerts child collection
    builder.HasMany(dc => dc.Alerts)
      .WithOne()
      .HasForeignKey(a => a.DailyClosingId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Metadata
      .FindNavigation(nameof(DailyClosing.Alerts))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(dc => new { dc.BusinessId, dc.Id }).IsUnique();
    builder.HasIndex(dc => new { dc.BusinessId, dc.BranchId, dc.ClosingDate }).IsUnique();
    builder.HasIndex(dc => new { dc.BusinessId, dc.ClosingDate });
    builder.HasIndex(dc => new { dc.BusinessId, dc.Status });
  }
}
