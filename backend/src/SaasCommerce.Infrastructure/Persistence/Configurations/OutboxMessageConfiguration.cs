using SaasCommerce.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasCommerce.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("outbox_messages");
    builder.HasKey(message => message.Id);

    builder.Property(message => message.BusinessId).IsRequired();
    builder.Property(message => message.Type).HasMaxLength(512).IsRequired();
    builder.Property(message => message.Content).IsRequired();
    builder.Property(message => message.OccurredOnUtc).IsRequired();

    builder.HasIndex(message => new { message.BusinessId, message.ProcessedOnUtc });
  }
}
