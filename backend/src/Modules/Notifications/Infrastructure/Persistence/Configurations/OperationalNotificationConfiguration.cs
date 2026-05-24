using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Infrastructure.Persistence.Configurations;

public sealed class OperationalNotificationConfiguration : IEntityTypeConfiguration<OperationalNotification>
{
  public void Configure(EntityTypeBuilder<OperationalNotification> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("operational_notifications", "notifications");

    builder.HasKey(n => n.Id);

    builder.Property(n => n.Id)
      .ValueGeneratedNever();

    builder.Property(n => n.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(n => n.BranchId);

    builder.Property(n => n.Type)
      .HasConversion<string>()
      .HasMaxLength(50)
      .IsRequired();

    builder.Property(n => n.Severity)
      .HasConversion<string>()
      .HasMaxLength(20)
      .IsRequired();

    builder.Property(n => n.Status)
      .HasConversion<string>()
      .HasMaxLength(20)
      .IsRequired();

    builder.Property(n => n.Title)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(n => n.Message)
      .HasMaxLength(1_000)
      .IsRequired();

    builder.Property(n => n.RelatedEntityId);

    builder.Property(n => n.RelatedEntityType)
      .HasMaxLength(100);

    builder.Property(n => n.CreatedAt).IsRequired();
    builder.Property(n => n.ReadAt);
    builder.Property(n => n.ReadByUserId);

    // Indexes
    builder.HasIndex(n => new { n.BusinessId, n.CreatedAt });
    builder.HasIndex(n => new { n.BusinessId, n.Status });
    builder.HasIndex(n => new { n.BusinessId, n.Type });
  }
}
