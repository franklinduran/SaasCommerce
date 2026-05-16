using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("refresh_tokens", "identity");

    builder.HasKey(token => token.Id);

    builder.Property(token => token.Id)
      .ValueGeneratedNever();

    builder.Property(token => token.UserId)
      .IsRequired();

    builder.Property(token => token.Token)
      .HasMaxLength(256)
      .IsRequired();

    builder.Property(token => token.ExpiresAt)
      .IsRequired();

    builder.Property(token => token.CreatedAt)
      .IsRequired();

    builder.Property(token => token.RevokedAt);

    builder.HasIndex(token => token.Token)
      .IsUnique();
  }
}
