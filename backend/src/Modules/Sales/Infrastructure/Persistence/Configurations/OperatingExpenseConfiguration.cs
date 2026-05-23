using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class OperatingExpenseConfiguration : IEntityTypeConfiguration<OperatingExpense>
{
  public void Configure(EntityTypeBuilder<OperatingExpense> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("operating_expenses", "sales");

    builder.HasKey(e => e.Id);

    builder.Property(e => e.Id)
      .ValueGeneratedNever();

    builder.Property(e => e.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(e => e.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(e => e.UserId)
      .IsRequired();

    builder.Property(e => e.CategoryId)
      .IsRequired();

    builder.Property(e => e.Description)
      .HasMaxLength(500)
      .IsRequired();

    builder.Property(e => e.Amount)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(e => e.PaymentMethod)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(e => e.Status)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(e => e.ExpenseDate)
      .IsRequired();

    builder.Property(e => e.Notes)
      .HasMaxLength(1_000);

    builder.Property(e => e.CashSessionId);

    builder.Property(e => e.CashMovementId);

    builder.Property(e => e.PaidAt);

    builder.Property(e => e.PaidByUserId);

    builder.Property(e => e.CancelledAt);

    builder.Property(e => e.CreatedAt)
      .IsRequired();

    builder.Property(e => e.UpdatedAt)
      .IsRequired();

    builder.HasIndex(e => new { e.BusinessId, e.Id })
      .IsUnique();

    builder.HasIndex(e => new { e.BusinessId, e.BranchId, e.Status });
    builder.HasIndex(e => new { e.BusinessId, e.CategoryId });
    builder.HasIndex(e => new { e.BusinessId, e.ExpenseDate });
    builder.HasIndex(e => new { e.BusinessId, e.Status, e.ExpenseDate });
  }
}
