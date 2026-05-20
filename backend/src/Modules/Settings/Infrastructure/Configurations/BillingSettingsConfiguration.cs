using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure.Configurations;

public sealed class BillingSettingsConfiguration : IEntityTypeConfiguration<BillingSettings>
{
  public void Configure(EntityTypeBuilder<BillingSettings> builder)
  {
    builder.ToTable("billing_settings", "settings");

    builder.HasKey(s => s.BusinessId);

    builder.Property(s => s.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(s => s.ReceiptHeaderText).HasMaxLength(500).IsRequired(false);
    builder.Property(s => s.ReceiptFooterText).HasMaxLength(500).IsRequired(false);
    builder.Property(s => s.ShowLogoOnReceipt).IsRequired().HasDefaultValue(false);
    builder.Property(s => s.ShowRncOnReceipt).IsRequired().HasDefaultValue(false);
    builder.Property(s => s.EnableInvoiceAutoGeneration).IsRequired().HasDefaultValue(true);

    builder.Property(s => s.InvoicePrefix)
      .HasMaxLength(10)
      .IsRequired()
      .HasDefaultValue("RI");

    builder.Property(s => s.InvoiceSequenceStart).IsRequired().HasDefaultValue(1);
    builder.Property(s => s.UpdatedBy).IsRequired();
    builder.Property(s => s.UpdatedAt).IsRequired();
  }
}
