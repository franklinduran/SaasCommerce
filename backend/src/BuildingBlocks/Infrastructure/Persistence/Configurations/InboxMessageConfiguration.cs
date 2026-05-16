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
    builder.HasKey(message => message.EventId);

    builder.Property(message => message.BusinessId).IsRequired();
    builder.Property(message => message.Type).HasMaxLength(512).IsRequired();
    builder.Property(message => message.ProcessedAt).IsRequired();

    builder.HasIndex(message => new { message.BusinessId, message.Type });
  }
}
