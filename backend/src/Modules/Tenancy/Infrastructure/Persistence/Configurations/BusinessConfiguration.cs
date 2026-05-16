using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
  public void Configure(EntityTypeBuilder<Business> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("businesses", "tenancy");

    builder.HasKey(business => business.Id);

    builder.Property(business => business.Id)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .ValueGeneratedNever();

    builder.Property(business => business.Name)
      .HasMaxLength(160)
      .IsRequired();

    builder.Property(business => business.IdentificationType)
      .HasConversion<string>()
      .HasMaxLength(40);

    builder.Property(business => business.IdentificationNumber)
      .HasMaxLength(40);

    builder.Property(business => business.IsActive)
      .IsRequired();

    builder.Property(business => business.CreatedAt)
      .IsRequired();

    builder.Property(business => business.UpdatedAt)
      .IsRequired();

    builder.HasIndex(business => new { business.IdentificationType, business.IdentificationNumber })
      .IsUnique()
      .HasFilter("\"IdentificationType\" IS NOT NULL AND \"IdentificationNumber\" IS NOT NULL");

    builder.HasMany(business => business.Branches)
      .WithOne()
      .HasForeignKey(branch => branch.BusinessId)
      .HasPrincipalKey(business => business.Id)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(business => business.Branches)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany(business => business.Phones)
      .WithOne()
      .HasForeignKey(phone => phone.BusinessId)
      .HasPrincipalKey(business => business.Id)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(business => business.Phones)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
