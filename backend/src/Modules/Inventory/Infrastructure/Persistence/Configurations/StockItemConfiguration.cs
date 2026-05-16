using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
  public void Configure(EntityTypeBuilder<StockItem> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("stock_items", "inventory");

    builder.HasKey(stockItem => stockItem.Id);

    builder.Property(stockItem => stockItem.Id)
      .ValueGeneratedNever();

    builder.Property(stockItem => stockItem.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(stockItem => stockItem.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(stockItem => stockItem.Quantity)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.Property(stockItem => stockItem.CreatedAt)
      .IsRequired();

    builder.HasIndex(stockItem => new { stockItem.BusinessId, stockItem.BranchId, stockItem.ProductId })
      .IsUnique();
  }
}
