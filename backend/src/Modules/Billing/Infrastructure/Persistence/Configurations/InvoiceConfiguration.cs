using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
  public void Configure(EntityTypeBuilder<Invoice> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("invoices", "billing");

    builder.HasKey(invoice => invoice.Id);

    builder.Property(invoice => invoice.Id)
      .ValueGeneratedNever();

    builder.Property(invoice => invoice.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(invoice => invoice.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(invoice => invoice.SaleId)
      .IsRequired();

    builder.Property(invoice => invoice.Sequence)
      .IsRequired();

    builder.Property(invoice => invoice.InvoiceNumber)
      .HasMaxLength(32)
      .IsRequired();

    builder.Property(invoice => invoice.Subtotal)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(invoice => invoice.DiscountTotal)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(invoice => invoice.TaxTotal)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(invoice => invoice.Total)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(invoice => invoice.Status)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(invoice => invoice.CreatedAt)
      .IsRequired();

    builder.Property(invoice => invoice.UpdatedAt)
      .IsRequired();

    builder.HasIndex(invoice => invoice.BusinessId);

    builder.HasIndex(invoice => new { invoice.BusinessId, invoice.SaleId })
      .IsUnique();

    builder.HasIndex(invoice => new { invoice.BusinessId, invoice.InvoiceNumber })
      .IsUnique();

    builder.HasIndex(invoice => new { invoice.BusinessId, invoice.CreatedAt });
  }
}
