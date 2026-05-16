using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("categories", "catalog");

    builder.HasKey(category => category.Id);

    builder.Property(category => category.Id)
      .ValueGeneratedNever();

    builder.Property(category => category.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(category => category.Name)
      .HasMaxLength(120)
      .IsRequired();

    builder.Property(category => category.Description)
      .HasMaxLength(300);

    builder.Property(category => category.IsActive)
      .IsRequired();

    builder.Property(category => category.CreatedAt)
      .IsRequired();

    builder.Property(category => category.UpdatedAt);

    builder.HasIndex(category => new { category.BusinessId, category.Name })
      .IsUnique();
  }
}
