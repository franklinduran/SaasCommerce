using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Billing.Domain;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SubscriptionPlan entity.
/// Defines table structure, constraints, and indexes.
/// </summary>
public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
  public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("subscription_plans", "billing");

    builder.HasKey(plan => plan.Id);

    builder.Property(plan => plan.Id)
      .HasColumnName("id")
      .ValueGeneratedNever()
      .IsRequired();

    builder.Property(plan => plan.Name)
      .HasColumnName("name")
      .HasMaxLength(128)
      .IsRequired();

    builder.Property(plan => plan.Code)
      .HasColumnName("code")
      .HasMaxLength(64)
      .IsRequired();

    builder.Property(plan => plan.Description)
      .HasColumnName("description")
      .HasMaxLength(1024)
      .HasDefaultValue(string.Empty);

    builder.Property(plan => plan.MonthlyPrice)
      .HasColumnName("monthly_price")
      .HasPrecision(10, 2)
      .HasDefaultValue(0m)
      .IsRequired();

    builder.Property(plan => plan.MaxBranches)
      .HasColumnName("max_branches")
      .HasDefaultValue(1)
      .IsRequired();

    builder.Property(plan => plan.MaxUsers)
      .HasColumnName("max_users")
      .HasDefaultValue(1)
      .IsRequired();

    builder.Property(plan => plan.MaxProducts)
      .HasColumnName("max_products")
      .HasDefaultValue(100)
      .IsRequired();

    builder.Property(plan => plan.MaxSalesPerMonth)
      .HasColumnName("max_sales_per_month")
      .HasDefaultValue(1000)
      .IsRequired();

    builder.Property(plan => plan.Features)
      .HasColumnName("features")
      .HasConversion<long>()
      .HasDefaultValue(SubscriptionFeature.None)
      .IsRequired();

    builder.Property(plan => plan.IsActive)
      .HasColumnName("is_active")
      .HasDefaultValue(true)
      .IsRequired();

    builder.Property(plan => plan.CreatedAt)
      .HasColumnName("created_at")
      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
      .IsRequired();

    builder.Property(plan => plan.UpdatedAt)
      .HasColumnName("updated_at")
      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'")
      .IsRequired();

    // Indexes for common queries
    builder.HasIndex(plan => plan.IsActive)
      .HasDatabaseName("ix_subscription_plans_is_active");

    builder.HasIndex(plan => plan.Code)
      .IsUnique()
      .HasDatabaseName("ix_subscription_plans_code_unique");

    builder.HasIndex(plan => plan.CreatedAt)
      .HasDatabaseName("ix_subscription_plans_created_at");

    builder.HasIndex(plan => new { plan.IsActive, plan.CreatedAt })
      .HasDatabaseName("ix_subscription_plans_active_created");
  }
}
