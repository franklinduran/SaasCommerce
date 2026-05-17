using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Purchasing.Domain;

namespace SaasCommerce.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
  public void Configure(EntityTypeBuilder<PurchaseItem> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("purchase_items", "purchasing");
    builder.HasKey(item => item.Id);

    builder.Property(item => item.Id)
      .ValueGeneratedNever();

    builder.Property(item => item.ProductId)
      .IsRequired();

    builder.Property(item => item.Quantity)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.Property(item => item.UnitCost)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Ignore(item => item.Subtotal);

    builder.HasIndex(item => new { item.PurchaseId, item.ProductId });
  }
}
