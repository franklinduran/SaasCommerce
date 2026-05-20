using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Infrastructure.Configurations;

public sealed class SalesSettingsConfiguration : IEntityTypeConfiguration<SalesSettings>
{
  public void Configure(EntityTypeBuilder<SalesSettings> builder)
  {
    builder.ToTable("sales_settings", "settings");

    builder.HasKey(s => s.BusinessId);

    builder.Property(s => s.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(s => s.AllowNegativeStock).IsRequired().HasDefaultValue(false);
    builder.Property(s => s.AllowDiscounts).IsRequired().HasDefaultValue(true);
    builder.Property(s => s.RequireCustomerForCreditSale).IsRequired().HasDefaultValue(true);
    builder.Property(s => s.DefaultPaymentMethod).HasMaxLength(50).IsRequired(false);
    builder.Property(s => s.EnableReceiptPrintAfterSale).IsRequired().HasDefaultValue(false);
    builder.Property(s => s.EnableInvoiceAutoGeneration).IsRequired().HasDefaultValue(true);
    builder.Property(s => s.UpdatedBy).IsRequired();
    builder.Property(s => s.UpdatedAt).IsRequired();
  }
}
