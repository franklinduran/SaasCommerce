using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("roles", "identity");

    builder.HasKey(role => role.Id);

    builder.Property(role => role.Id)
      .ValueGeneratedNever();

    builder.Property(role => role.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(role => role.Name)
      .HasMaxLength(80)
      .IsRequired();

    builder.HasIndex(role => new { role.BusinessId, role.Name })
      .IsUnique();
  }
}
