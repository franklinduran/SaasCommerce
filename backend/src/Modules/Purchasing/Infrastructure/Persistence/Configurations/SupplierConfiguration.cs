using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
  public void Configure(EntityTypeBuilder<Supplier> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("suppliers", "purchasing");
    builder.HasKey(supplier => supplier.Id);

    builder.Property(supplier => supplier.Id)
      .ValueGeneratedNever();

    builder.Property(supplier => supplier.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(supplier => supplier.Name)
      .HasMaxLength(SupplierRules.NameMaxLength)
      .IsRequired();

    builder.Property(supplier => supplier.SearchName)
      .HasMaxLength(SupplierRules.SearchNameMaxLength)
      .IsRequired();

    builder.Property(supplier => supplier.Rnc)
      .HasMaxLength(SupplierRules.RncMaxLength);

    builder.Property(supplier => supplier.Phone)
      .HasMaxLength(SupplierRules.PhoneMaxLength);

    builder.Property(supplier => supplier.Email)
      .HasMaxLength(SupplierRules.EmailMaxLength);

    builder.Property(supplier => supplier.Address)
      .HasMaxLength(SupplierRules.AddressMaxLength);

    builder.Property(supplier => supplier.IsActive)
      .IsRequired();

    builder.Property(supplier => supplier.CreatedAt)
      .IsRequired();

    builder.HasIndex(supplier => new { supplier.BusinessId, supplier.Name });
    builder.HasIndex(supplier => new { supplier.BusinessId, supplier.SearchName });
    builder.HasIndex(supplier => new { supplier.BusinessId, supplier.Rnc });
    builder.HasIndex(supplier => new { supplier.BusinessId, supplier.IsActive });
  }
}
