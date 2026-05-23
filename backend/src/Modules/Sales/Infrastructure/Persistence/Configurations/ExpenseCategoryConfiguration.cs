using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
  public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("expense_categories", "sales");

    builder.HasKey(c => c.Id);

    builder.Property(c => c.Id)
      .ValueGeneratedNever();

    builder.Property(c => c.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(c => c.Name)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(c => c.IsActive)
      .IsRequired();

    builder.Property(c => c.CreatedAt)
      .IsRequired();

    builder.Property(c => c.UpdatedAt)
      .IsRequired();

    builder.HasIndex(c => new { c.BusinessId, c.Id })
      .IsUnique();

    builder.HasIndex(c => new { c.BusinessId, c.Name })
      .IsUnique();

    builder.HasIndex(c => new { c.BusinessId, c.IsActive });
  }
}
