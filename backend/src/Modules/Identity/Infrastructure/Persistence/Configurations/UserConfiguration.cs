using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("users", "identity");

    builder.HasKey(user => user.Id);

    builder.Property(user => user.Id)
      .ValueGeneratedNever();

    builder.Property(user => user.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(user => user.DefaultBranchId)
      .HasConversion(
        id => id.HasValue ? id.Value.Value : (Guid?)null,
        value => value.HasValue ? new BranchId(value.Value) : null);

    builder.Property(user => user.FullName)
      .HasMaxLength(160)
      .IsRequired();

    builder.Property(user => user.Email)
      .HasMaxLength(320)
      .IsRequired();

    builder.Property(user => user.Phone)
      .HasMaxLength(40);

    builder.Property(user => user.PasswordHash)
      .HasMaxLength(512)
      .IsRequired();

    builder.Property(user => user.IsActive)
      .IsRequired();

    builder.Property(user => user.MustChangePassword)
      .IsRequired()
      .HasDefaultValue(false);

    builder.Property(user => user.CreatedAt)
      .IsRequired();

    builder.Property(user => user.UpdatedAt)
      .IsRequired();

    builder.HasIndex(user => new { user.BusinessId, user.Email })
      .IsUnique();

    builder.HasMany(user => user.Roles)
      .WithMany()
      .UsingEntity<Dictionary<string, object>>(
        "user_roles",
        right => right.HasOne<Role>()
          .WithMany()
          .HasForeignKey("role_id")
          .OnDelete(DeleteBehavior.Cascade),
        left => left.HasOne<User>()
          .WithMany()
          .HasForeignKey("user_id")
          .OnDelete(DeleteBehavior.Cascade),
        join =>
        {
          join.ToTable("user_roles", "identity");
          join.HasKey("user_id", "role_id");
        });

    builder.HasMany(user => user.RefreshTokens)
      .WithOne()
      .HasForeignKey(token => token.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(user => user.Roles)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.Navigation(user => user.RefreshTokens)
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}
