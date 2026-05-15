using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasCommerce.Modules.Tenancy.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
  public void Configure(EntityTypeBuilder<Branch> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("branches", "tenancy");

    builder.HasKey(branch => branch.Id);

    builder.Property(branch => branch.Id)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .ValueGeneratedNever();

    builder.Property(branch => branch.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(branch => branch.Name)
      .HasMaxLength(160)
      .IsRequired();

    builder.Property(branch => branch.IsMain)
      .IsRequired();

    builder.Property(branch => branch.IsActive)
      .IsRequired();

    builder.Property(branch => branch.CreatedAt)
      .IsRequired();

    builder.HasIndex(branch => new { branch.BusinessId, branch.Name })
      .IsUnique();
  }
}
