using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
  public void Configure(EntityTypeBuilder<Purchase> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("purchases", "purchasing");
    builder.HasKey(purchase => purchase.Id);

    builder.Property(purchase => purchase.Id)
      .ValueGeneratedNever();

    builder.Property(purchase => purchase.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(purchase => purchase.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(purchase => purchase.Status)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(purchase => purchase.SupplierInvoiceNumber)
      .HasMaxLength(80);

    builder.Property(purchase => purchase.Notes)
      .HasMaxLength(1_000);

    builder.Property(purchase => purchase.FailureReason)
      .HasMaxLength(500);

    builder.Property(purchase => purchase.Total)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(purchase => purchase.PurchaseDate)
      .IsRequired();

    builder.Property(purchase => purchase.CreatedAt)
      .IsRequired();

    builder.Property(purchase => purchase.UpdatedAt)
      .IsRequired();

    builder.HasMany(purchase => purchase.Items)
      .WithOne()
      .HasForeignKey(item => item.PurchaseId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Metadata
      .FindNavigation(nameof(Purchase.Items))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(purchase => new { purchase.BusinessId, purchase.Id })
      .IsUnique();
    builder.HasIndex(purchase => new { purchase.BusinessId, purchase.BranchId, purchase.Status });
    builder.HasIndex(purchase => new { purchase.BusinessId, purchase.SupplierId });
    builder.HasIndex(purchase => new { purchase.BusinessId, purchase.PurchaseDate });
    builder.HasIndex(purchase => new { purchase.BusinessId, purchase.SupplierInvoiceNumber });
  }
}
