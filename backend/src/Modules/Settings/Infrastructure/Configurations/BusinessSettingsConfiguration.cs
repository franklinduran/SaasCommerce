using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure.Configurations;

public sealed class BusinessSettingsConfiguration : IEntityTypeConfiguration<BusinessSettings>
{
  public void Configure(EntityTypeBuilder<BusinessSettings> builder)
  {
    builder.ToTable("business_settings", "settings");

    builder.HasKey(s => s.BusinessId);

    builder.Property(s => s.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(s => s.CommercialName).HasMaxLength(200).IsRequired(false);
    builder.Property(s => s.LegalName).HasMaxLength(200).IsRequired(false);
    builder.Property(s => s.Rnc).HasMaxLength(20).IsRequired(false);
    builder.Property(s => s.Phone).HasMaxLength(30).IsRequired(false);
    builder.Property(s => s.Email).HasMaxLength(254).IsRequired(false);
    builder.Property(s => s.Address).HasMaxLength(500).IsRequired(false);

    builder.Property(s => s.Currency)
      .HasMaxLength(3)
      .IsRequired()
      .HasDefaultValue("DOP");

    builder.Property(s => s.Timezone)
      .HasMaxLength(80)
      .IsRequired()
      .HasDefaultValue("America/Santo_Domingo");

    builder.Property(s => s.LogoUrl).HasMaxLength(2048).IsRequired(false);
    builder.Property(s => s.ReceiptFooterText).HasMaxLength(500).IsRequired(false);
    builder.Property(s => s.UpdatedBy).IsRequired();
    builder.Property(s => s.UpdatedAt).IsRequired();
  }
}
