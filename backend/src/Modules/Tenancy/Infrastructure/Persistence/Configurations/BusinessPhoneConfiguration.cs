using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Infrastructure.Persistence.Configurations;

public sealed class BusinessPhoneConfiguration : IEntityTypeConfiguration<BusinessPhone>
{
  public void Configure(EntityTypeBuilder<BusinessPhone> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("business_phones", "tenancy");

    builder.HasKey(phone => phone.Id);

    builder.Property(phone => phone.Id)
      .ValueGeneratedNever();

    builder.Property(phone => phone.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(phone => phone.Number)
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(phone => phone.Label)
      .HasMaxLength(80);

    builder.Property(phone => phone.IsPrimary)
      .IsRequired();

    builder.Property(phone => phone.CreatedAt)
      .IsRequired();

    builder.HasIndex(phone => new { phone.BusinessId, phone.Number })
      .IsUnique();
  }
}
