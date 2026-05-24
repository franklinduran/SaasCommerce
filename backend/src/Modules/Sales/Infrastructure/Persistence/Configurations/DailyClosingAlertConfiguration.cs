using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class DailyClosingAlertConfiguration : IEntityTypeConfiguration<DailyClosingAlert>
{
  public void Configure(EntityTypeBuilder<DailyClosingAlert> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("daily_closing_alerts", "sales");

    builder.HasKey(a => a.Id);

    builder.Property(a => a.Id)
      .ValueGeneratedNever();

    builder.Property(a => a.DailyClosingId)
      .IsRequired();

    builder.Property(a => a.AlertType)
      .HasConversion<string>()
      .HasMaxLength(50)
      .IsRequired();

    builder.Property(a => a.Message)
      .HasMaxLength(500)
      .IsRequired();

    builder.Property(a => a.EstimatedImpact)
      .HasPrecision(18, 2);

    builder.HasIndex(a => a.DailyClosingId);
  }
}
