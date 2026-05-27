using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Feedback.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Feedback.Infrastructure.Persistence.Configurations;

public sealed class BetaFeedbackConfiguration : IEntityTypeConfiguration<BetaFeedback>
{
  public void Configure(EntityTypeBuilder<BetaFeedback> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("beta_feedback", "feedback");
    builder.HasKey(feedback => feedback.Id);
    builder.Property(feedback => feedback.Id).ValueGeneratedNever();
    builder.Property(feedback => feedback.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();
    builder.Property(feedback => feedback.UserId).IsRequired();
    builder.Property(feedback => feedback.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
    builder.Property(feedback => feedback.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
    builder.Property(feedback => feedback.Title).HasMaxLength(160).IsRequired();
    builder.Property(feedback => feedback.Description).HasMaxLength(2_000).IsRequired();
    builder.Property(feedback => feedback.ContextUrl).HasMaxLength(500);
    builder.Property(feedback => feedback.ReviewNote).HasMaxLength(1_000);
    builder.Property(feedback => feedback.CreatedAt).IsRequired();
    builder.Property(feedback => feedback.UpdatedAt).IsRequired();
    builder.Property(feedback => feedback.ReviewedAt);
    builder.Property(feedback => feedback.ReviewedByUserId);

    builder.HasIndex(feedback => new { feedback.BusinessId, feedback.Status, feedback.CreatedAt });
    builder.HasIndex(feedback => new { feedback.BusinessId, feedback.Category });
    builder.HasIndex(feedback => new { feedback.BusinessId, feedback.UserId });
  }
}
