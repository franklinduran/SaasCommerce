using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("outbox_messages");
    builder.HasKey(message => message.Id);

    builder.Property(message => message.EventId).IsRequired();
    builder.Property(message => message.CorrelationId).IsRequired();
    builder.Property(message => message.BusinessId).IsRequired();
    builder.Property(message => message.EventType).HasMaxLength(512).IsRequired();
    builder.Property(message => message.Payload).IsRequired();
    builder.Property(message => message.OccurredAt).IsRequired();
    builder.Property(message => message.PublishedAt);
    builder.Property(message => message.Attempts).IsRequired();
    builder.Property(message => message.LastError).HasMaxLength(2_000);
    builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
    builder.Property(message => message.CreatedAt).IsRequired();

    builder.HasIndex(message => new { message.Status, message.OccurredAt });
    builder.HasIndex(message => new { message.BusinessId, message.Status });
    builder.HasIndex(message => message.EventId).IsUnique();
  }
}
