using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
  public void Configure(EntityTypeBuilder<CashSession> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("cash_sessions", "sales");

    builder.HasKey(cs => cs.Id);

    builder.Property(cs => cs.Id)
      .ValueGeneratedNever();

    builder.Property(cs => cs.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(cs => cs.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(cs => cs.UserId)
      .IsRequired();

    builder.Property(cs => cs.Status)
      .HasConversion<string>()
      .HasMaxLength(40)
      .IsRequired();

    builder.Property(cs => cs.OpeningBalance)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(cs => cs.ClosingBalance)
      .HasPrecision(18, 2);

    builder.Property(cs => cs.Notes)
      .HasMaxLength(1_000);

    builder.Property(cs => cs.OpenedAt)
      .IsRequired();

    builder.Property(cs => cs.ClosedAt);

    builder.Property(cs => cs.UpdatedAt)
      .IsRequired();

    builder.HasMany(cs => cs.Movements)
      .WithOne()
      .HasForeignKey(m => m.CashSessionId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Metadata
      .FindNavigation(nameof(CashSession.Movements))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(cs => new { cs.BusinessId, cs.Id })
      .IsUnique();

    builder.HasIndex(cs => new { cs.BusinessId, cs.BranchId, cs.Status });
    builder.HasIndex(cs => new { cs.BusinessId, cs.OpenedAt });
  }
}
