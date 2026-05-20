using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure.Configurations;

public sealed class InventorySettingsConfiguration : IEntityTypeConfiguration<InventorySettings>
{
  public void Configure(EntityTypeBuilder<InventorySettings> builder)
  {
    builder.ToTable("inventory_settings", "settings");

    builder.HasKey(s => s.BusinessId);

    builder.Property(s => s.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(s => s.EnableLowStockAlerts).IsRequired().HasDefaultValue(true);

    builder.Property(s => s.DefaultLowStockThreshold)
      .IsRequired()
      .HasColumnType("numeric(18,4)")
      .HasDefaultValue(5m);

    builder.Property(s => s.RequireReasonForInventoryAdjustment).IsRequired().HasDefaultValue(false);
    builder.Property(s => s.AllowInventoryTransferBetweenBranches).IsRequired().HasDefaultValue(false);
    builder.Property(s => s.UpdatedBy).IsRequired();
    builder.Property(s => s.UpdatedAt).IsRequired();
  }
}
