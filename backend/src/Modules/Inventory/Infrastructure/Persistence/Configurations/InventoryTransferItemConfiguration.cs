using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Inventory.Domain;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryTransferItemConfiguration : IEntityTypeConfiguration<InventoryTransferItem>
{
  public void Configure(EntityTypeBuilder<InventoryTransferItem> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("inventory_transfer_items", "inventory");

    builder.HasKey(item => new { item.TransferId, item.ProductId });

    builder.Property(item => item.TransferId)
      .IsRequired();

    builder.Property(item => item.ProductId)
      .IsRequired();

    builder.Property(item => item.Quantity)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.HasIndex(item => item.TransferId);
  }
}
