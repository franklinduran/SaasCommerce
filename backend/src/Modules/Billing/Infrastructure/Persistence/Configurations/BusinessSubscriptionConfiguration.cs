using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for BusinessSubscription entity.
/// Defines table structure, constraints, and indexes.
/// Maintains referential integrity with SubscriptionPlan.
/// </summary>
public sealed class BusinessSubscriptionConfiguration : IEntityTypeConfiguration<BusinessSubscription>
{
  public void Configure(EntityTypeBuilder<BusinessSubscription> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("business_subscriptions", "billing");

    builder.HasKey(sub => sub.Id);

    builder.Property(sub => sub.Id)
      .HasColumnName("id")
      .ValueGeneratedNever()
      .IsRequired();

    builder.Property(sub => sub.BusinessId)
      .HasColumnName("business_id")
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(sub => sub.PlanId)
      .HasColumnName("plan_id")
      .IsRequired();

    builder.Property(sub => sub.Status)
      .HasColumnName("status")
      .HasConversion<string>()
      .HasMaxLength(40)
      .HasDefaultValue(SubscriptionStatus.Trial)
      .IsRequired();

    builder.Property(sub => sub.StartedAt)
      .HasColumnName("started_at")
      .IsRequired();

    builder.Property(sub => sub.TrialEndsAt)
      .HasColumnName("trial_ends_at")
      .IsRequired(false);

    builder.Property(sub => sub.CurrentPeriodStart)
      .HasColumnName("current_period_start")
      .IsRequired(false);

    builder.Property(sub => sub.CurrentPeriodEnd)
      .HasColumnName("current_period_end")
      .IsRequired(false);

    builder.Property(sub => sub.CancelledAt)
      .HasColumnName("cancelled_at")
      .IsRequired(false);

    builder.Property(sub => sub.SuspendedAt)
      .HasColumnName("suspended_at")
      .IsRequired(false);

    builder.Property(sub => sub.CancellationReason)
      .HasColumnName("cancellation_reason")
      .HasMaxLength(512)
      .IsRequired(false);

    builder.Property(sub => sub.CreatedAt)
      .HasColumnName("created_at")
      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
      .IsRequired();

    builder.Property(sub => sub.UpdatedAt)
      .HasColumnName("updated_at")
      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
      .IsRequired();

    // Unique constraint: one subscription per business
    builder.HasIndex(sub => sub.BusinessId)
      .IsUnique()
      .HasDatabaseName("ix_business_subscriptions_business_id_unique");

    // Indexes for common queries
    builder.HasIndex(sub => sub.Status)
      .HasDatabaseName("ix_business_subscriptions_status");

    builder.HasIndex(sub => sub.PlanId)
      .HasDatabaseName("ix_business_subscriptions_plan_id");

    builder.HasIndex(sub => sub.TrialEndsAt)
      .HasDatabaseName("ix_business_subscriptions_trial_ends_at");

    builder.HasIndex(sub => sub.CurrentPeriodEnd)
      .HasDatabaseName("ix_business_subscriptions_current_period_end");

    builder.HasIndex(sub => new { sub.Status, sub.CurrentPeriodEnd })
      .HasDatabaseName("ix_business_subscriptions_status_period");

    // Foreign key to SubscriptionPlan (soft constraint - plan could be deleted)
    builder.HasOne<SubscriptionPlan>()
      .WithMany()
      .HasForeignKey(sub => sub.PlanId)
      .IsRequired()
      .OnDelete(DeleteBehavior.Restrict)
      .HasConstraintName("fk_business_subscriptions_subscription_plans");
  }
}
