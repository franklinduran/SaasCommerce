using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class PosCartConfiguration : IEntityTypeConfiguration<PosCart>
{
  public void Configure(EntityTypeBuilder<PosCart> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("pos_carts", "sales");
    builder.HasKey(c => c.Id);
    builder.Property(c => c.Id).ValueGeneratedNever();

    builder.Property(c => c.BusinessId)
      .HasConversion(id => id.Value, v => new BusinessId(v))
      .IsRequired();

    builder.Property(c => c.UserId).IsRequired();
    builder.Property(c => c.UpdatedAt).IsRequired();

    builder.HasMany(c => c.Items)
      .WithOne()
      .HasForeignKey(i => i.CartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(c => new { c.BusinessId, c.UserId }).IsUnique();
  }
}

public sealed class PosCartItemConfiguration : IEntityTypeConfiguration<PosCartItem>
{
  public void Configure(EntityTypeBuilder<PosCartItem> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("pos_cart_items", "sales");
    builder.HasKey(i => i.Id);
    builder.Property(i => i.Id).ValueGeneratedNever();
    builder.Property(i => i.Name).HasMaxLength(180).IsRequired();
    builder.Property(i => i.Sku).HasMaxLength(80).IsRequired();
    builder.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
    builder.Property(i => i.Quantity).IsRequired();

    builder.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();
  }
}
