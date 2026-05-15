using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("products", "catalog");

    builder.HasKey(product => product.Id);

    builder.Property(product => product.Id)
      .ValueGeneratedNever();

    builder.Property(product => product.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(product => product.ProductType)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(product => product.Name)
      .HasMaxLength(180)
      .IsRequired();

    builder.Property(product => product.Description)
      .HasMaxLength(600);

    builder.Property(product => product.Sku)
      .HasMaxLength(80)
      .IsRequired();

    builder.Property(product => product.Barcode)
      .HasMaxLength(80);

    builder.Property(product => product.SearchName)
      .HasMaxLength(180)
      .IsRequired();

    builder.Property(product => product.InternalCode)
      .HasMaxLength(80);

    builder.Property(product => product.SupplierCode)
      .HasMaxLength(80);

    builder.Property(product => product.UnitOfMeasure)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(product => product.SalePrice)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(product => product.CostPrice)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(product => product.WholesalePrice)
      .HasPrecision(18, 2);

    builder.Property(product => product.MinSalePrice)
      .HasPrecision(18, 2);

    builder.Property(product => product.TaxCategory)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(product => product.TaxRate)
      .HasPrecision(5, 2)
      .IsRequired();

    builder.Property(product => product.MinimumStock)
      .HasPrecision(18, 3);

    builder.Property(product => product.MaximumStock)
      .HasPrecision(18, 3);

    builder.Property(product => product.ReorderPoint)
      .HasPrecision(18, 3);

    builder.Property(product => product.VariantName)
      .HasMaxLength(120);

    builder.Property(product => product.AttributesJson)
      .HasColumnType("jsonb");

    builder.Property(product => product.IsActive)
      .IsRequired();

    builder.Property(product => product.CreatedAt)
      .IsRequired();

    builder.HasIndex(product => new { product.BusinessId, product.Sku })
      .IsUnique();

    builder.HasIndex(product => new { product.BusinessId, product.Barcode })
      .IsUnique()
      .HasFilter("\"Barcode\" IS NOT NULL");

    builder.HasIndex(product => new { product.BusinessId, product.SearchName });
  }
}
