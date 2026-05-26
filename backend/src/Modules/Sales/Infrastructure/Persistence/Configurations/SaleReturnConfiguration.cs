using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class SaleReturnConfiguration : IEntityTypeConfiguration<SaleReturn>
{
  public void Configure(EntityTypeBuilder<SaleReturn> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("sale_returns", "sales");
    builder.HasKey(saleReturn => saleReturn.Id);
    builder.Property(saleReturn => saleReturn.Id).ValueGeneratedNever();
    builder.Property(saleReturn => saleReturn.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();
    builder.Property(saleReturn => saleReturn.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();
    builder.Property(saleReturn => saleReturn.Status)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();
    builder.Property(saleReturn => saleReturn.Reason)
      .HasMaxLength(500)
      .IsRequired();
    builder.Property(saleReturn => saleReturn.Total)
      .HasPrecision(18, 2)
      .IsRequired();
    builder.Property(saleReturn => saleReturn.FailureReason)
      .HasMaxLength(500);
    builder.Property(saleReturn => saleReturn.RequestedAt).IsRequired();
    builder.Property(saleReturn => saleReturn.UpdatedAt).IsRequired();

    builder.HasMany(saleReturn => saleReturn.Items)
      .WithOne()
      .HasForeignKey(item => item.SaleReturnId)
      .OnDelete(DeleteBehavior.Cascade);
    builder.Metadata.FindNavigation(nameof(SaleReturn.Items))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(saleReturn => new { saleReturn.BusinessId, saleReturn.SaleId });
    builder.HasIndex(saleReturn => new { saleReturn.BusinessId, saleReturn.Status });
  }
}
