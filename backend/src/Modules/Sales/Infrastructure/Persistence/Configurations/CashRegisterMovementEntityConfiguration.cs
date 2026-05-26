using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

internal sealed class CashRegisterMovementEntityConfiguration : IEntityTypeConfiguration<CashRegisterMovement>
{
  public void Configure(EntityTypeBuilder<CashRegisterMovement> builder)
  {
    builder.ToTable("cash_register_movements", "sales");

    builder.HasKey(m => m.Id);

    builder.Property(m => m.Id).HasColumnName("Id").IsRequired();
    builder.Property(m => m.CashRegisterId).HasColumnName("CashRegisterId").IsRequired();
    builder.Property(m => m.UserId).HasColumnName("UserId").IsRequired();
    builder.Property(m => m.MovementType)
      .HasColumnName("MovementType")
      .HasConversion<string>()
      .HasMaxLength(20)
      .IsRequired();
    builder.Property(m => m.Amount)
      .HasColumnName("Amount")
      .HasPrecision(18, 2)
      .IsRequired();
    builder.Property(m => m.Reason)
      .HasColumnName("Reason")
      .HasMaxLength(500)
      .IsRequired();
    builder.Property(m => m.CreatedAt).HasColumnName("CreatedAt").IsRequired();

    builder.HasIndex(m => m.CashRegisterId)
      .HasDatabaseName("IX_cash_register_movements_CashRegisterId");
  }
}
