using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
  public void Configure(EntityTypeBuilder<Sale> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("sales", "sales");

    builder.HasKey(sale => sale.Id);

    builder.Property(sale => sale.Id)
      .ValueGeneratedNever();

    builder.Property(sale => sale.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(sale => sale.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(sale => sale.Status)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(sale => sale.PaymentMethod)
      .HasMaxLength(80)
      .IsRequired();

    builder.Property(sale => sale.CustomerId);

    builder.Property(sale => sale.Total)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(sale => sale.CreatedAt)
      .IsRequired();

    builder.Property(sale => sale.UpdatedAt)
      .IsRequired();

    builder.Property(sale => sale.FailureReason)
      .HasMaxLength(1_000);

    builder.Property(sale => sale.CancellationReason)
      .HasMaxLength(1_000);

    builder.HasMany(sale => sale.Items)
      .WithOne()
      .HasForeignKey(item => item.SaleId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Metadata
      .FindNavigation(nameof(Sale.Items))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(sale => new { sale.BusinessId, sale.Id })
      .IsUnique();

    builder.HasIndex(sale => new { sale.BusinessId, sale.BranchId, sale.Status });
    builder.HasIndex(sale => new { sale.BusinessId, sale.CustomerId });
  }
}
