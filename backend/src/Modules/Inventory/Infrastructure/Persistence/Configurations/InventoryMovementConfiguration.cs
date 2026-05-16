using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
  public void Configure(EntityTypeBuilder<InventoryMovement> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("inventory_movements", "inventory");

    builder.HasKey(movement => movement.Id);

    builder.Property(movement => movement.Id)
      .ValueGeneratedNever();

    builder.Property(movement => movement.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(movement => movement.PreviousStock)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.Property(movement => movement.NewStock)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.Property(movement => movement.Quantity)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.Property(movement => movement.Reason)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(movement => movement.CreatedAt)
      .IsRequired();

    builder.HasIndex(movement => new { movement.BusinessId, movement.ProductId, movement.CreatedAt });
  }
}
