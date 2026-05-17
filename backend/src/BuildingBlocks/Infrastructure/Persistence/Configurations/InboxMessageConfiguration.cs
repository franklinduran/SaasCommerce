using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
  public void Configure(EntityTypeBuilder<InboxMessage> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("inbox_messages");
    builder.HasKey(message => message.Id);

    builder.Property(message => message.EventId).IsRequired();
    builder.Property(message => message.ConsumerName).HasMaxLength(256).IsRequired();
    builder.Property(message => message.BusinessId).IsRequired();
    builder.Property(message => message.CorrelationId).IsRequired();
    builder.Property(message => message.ProcessedAt).IsRequired();
    builder.Property(message => message.CreatedAt).IsRequired();

    builder.HasIndex(message => new { message.EventId, message.ConsumerName }).IsUnique();
    builder.HasIndex(message => new { message.BusinessId, message.ConsumerName });
  }
}
