using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
  public void Configure(EntityTypeBuilder<CreditNote> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("credit_notes", "sales");
    builder.HasKey(note => note.Id);
    builder.Property(note => note.Id).ValueGeneratedNever();
    builder.Property(note => note.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();
    builder.Property(note => note.BranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();
    builder.Property(note => note.Code)
      .HasMaxLength(60)
      .IsRequired();
    builder.Property(note => note.Total)
      .HasPrecision(18, 2)
      .IsRequired();
    builder.Property(note => note.CreatedAt).IsRequired();

    builder.HasMany(note => note.Items)
      .WithOne()
      .HasForeignKey(item => item.CreditNoteId)
      .OnDelete(DeleteBehavior.Cascade);
    builder.Metadata.FindNavigation(nameof(CreditNote.Items))!
      .SetPropertyAccessMode(PropertyAccessMode.Field);

    builder.HasIndex(note => new { note.BusinessId, note.SaleReturnId }).IsUnique();
    builder.HasIndex(note => new { note.BusinessId, note.Code }).IsUnique();
  }
}
