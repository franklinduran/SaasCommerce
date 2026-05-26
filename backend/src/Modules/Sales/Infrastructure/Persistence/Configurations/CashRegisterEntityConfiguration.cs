using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CashRegisterEntityConfiguration : IEntityTypeConfiguration<CashRegister>
{
  public void Configure(EntityTypeBuilder<CashRegister> builder)
  {
    builder.ToTable("cash_registers", "sales");

    builder.HasKey(r => r.Id);

    builder.Property(r => r.Id).HasColumnName("Id").IsRequired();
    builder.Property(r => r.BusinessId)
      .HasColumnName("BusinessId")
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();
    builder.Property(r => r.BranchId)
      .HasColumnName("BranchId")
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();
    builder.Property(r => r.UserId).HasColumnName("UserId").IsRequired();
    builder.Property(r => r.Status)
      .HasColumnName("Status")
      .HasConversion<string>()
      .HasMaxLength(20)
      .IsRequired();
    builder.Property(r => r.OpeningAmount)
      .HasColumnName("OpeningAmount")
      .HasPrecision(18, 2)
      .IsRequired();
    builder.Property(r => r.Notes).HasColumnName("Notes").HasMaxLength(500);
    builder.Property(r => r.OpenedAt).HasColumnName("OpenedAt").IsRequired();
    builder.Property(r => r.ClosedAt).HasColumnName("ClosedAt");
    builder.Property(r => r.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();

    // Close-time snapshot fields
    builder.Property(r => r.CountedAmount).HasColumnName("CountedAmount").HasPrecision(18, 2);
    builder.Property(r => r.ExpectedCashAmount).HasColumnName("ExpectedCashAmount").HasPrecision(18, 2);
    builder.Property(r => r.Difference).HasColumnName("Difference").HasPrecision(18, 2);
    builder.Property(r => r.DifferenceType)
      .HasColumnName("DifferenceType")
      .HasConversion<string>()
      .HasMaxLength(20);
    builder.Property(r => r.CloseNotes).HasColumnName("CloseNotes").HasMaxLength(1000);

    builder.Property(r => r.CashSalesTotal).HasColumnName("CashSalesTotal").HasPrecision(18, 2).IsRequired();
    builder.Property(r => r.CardSalesTotal).HasColumnName("CardSalesTotal").HasPrecision(18, 2).IsRequired();
    builder.Property(r => r.TransferSalesTotal).HasColumnName("TransferSalesTotal").HasPrecision(18, 2).IsRequired();
    builder.Property(r => r.CreditSalesTotal).HasColumnName("CreditSalesTotal").HasPrecision(18, 2).IsRequired();
    builder.Property(r => r.CashReturnsTotal).HasColumnName("CashReturnsTotal").HasPrecision(18, 2).IsRequired();

    builder.Ignore(r => r.ManualCashIn);
    builder.Ignore(r => r.ManualCashOut);

    builder.HasMany(r => r.Movements)
      .WithOne()
      .HasForeignKey(m => m.CashRegisterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(r => new { r.BusinessId, r.BranchId, r.Status })
      .HasDatabaseName("IX_cash_registers_BusinessId_BranchId_Status");

    builder.HasIndex(r => new { r.BusinessId, r.UserId, r.Status })
      .HasDatabaseName("IX_cash_registers_BusinessId_UserId_Status");

    builder.HasIndex(r => new { r.BusinessId, r.Id })
      .HasDatabaseName("IX_cash_registers_BusinessId_Id")
      .IsUnique();

    builder.HasIndex(r => new { r.BusinessId, r.OpenedAt })
      .HasDatabaseName("IX_cash_registers_BusinessId_OpenedAt");
  }
}
