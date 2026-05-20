using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
  public void Configure(EntityTypeBuilder<AuditLog> builder)
  {
    builder.ToTable("audit_logs", "identity");

    builder.HasKey(log => log.Id);

    builder.Property(log => log.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(log => log.UserId).IsRequired(false);

    builder.Property(log => log.Action)
      .HasMaxLength(120)
      .IsRequired();

    builder.Property(log => log.EntityName)
      .HasMaxLength(80)
      .IsRequired();

    builder.Property(log => log.EntityId).IsRequired(false);

    builder.Property(log => log.Description)
      .HasMaxLength(500)
      .IsRequired(false);

    builder.Property(log => log.IpAddress)
      .HasMaxLength(45)
      .IsRequired(false);

    builder.Property(log => log.CorrelationId).IsRequired(false);

    builder.Property(log => log.UserAgent)
      .HasMaxLength(512)
      .IsRequired(false);

    builder.Property(log => log.MetadataJson)
      .HasMaxLength(2000)
      .IsRequired(false);

    builder.Property(log => log.CreatedAt).IsRequired();

    builder.HasIndex(log => log.BusinessId).HasDatabaseName("ix_audit_logs_business_id");
    builder.HasIndex(log => new { log.BusinessId, log.CreatedAt })
      .HasDatabaseName("ix_audit_logs_business_id_created_at");
    builder.HasIndex(log => new { log.BusinessId, log.UserId })
      .HasDatabaseName("ix_audit_logs_business_id_user_id");
    builder.HasIndex(log => new { log.BusinessId, log.CorrelationId })
      .HasDatabaseName("ix_audit_logs_business_id_correlation_id");
  }
}
