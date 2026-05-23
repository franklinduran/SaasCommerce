using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
  public void Configure(EntityTypeBuilder<CashMovement> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("cash_movements", "sales");

    builder.HasKey(m => m.Id);

    builder.Property(m => m.Id)
      .ValueGeneratedNever();

    builder.Property(m => m.CashSessionId)
      .IsRequired();

    builder.Property(m => m.UserId)
      .IsRequired();

    builder.Property(m => m.Type)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(m => m.Amount)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(m => m.Description)
      .HasMaxLength(500)
      .IsRequired();

    builder.Property(m => m.CreatedAt)
      .IsRequired();

    builder.HasIndex(m => m.CashSessionId);
    builder.HasIndex(m => new { m.CashSessionId, m.CreatedAt });
  }
}
