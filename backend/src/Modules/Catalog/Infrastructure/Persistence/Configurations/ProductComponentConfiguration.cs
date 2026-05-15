using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductComponentConfiguration : IEntityTypeConfiguration<ProductComponent>
{
  public void Configure(EntityTypeBuilder<ProductComponent> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("product_components", "catalog");

    builder.HasKey(component => component.Id);

    builder.Property(component => component.Id)
      .ValueGeneratedNever();

    builder.Property(component => component.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(component => component.Quantity)
      .HasPrecision(18, 3)
      .IsRequired();

    builder.HasIndex(component => new { component.BusinessId, component.ProductId, component.ComponentProductId })
      .IsUnique();
  }
}
