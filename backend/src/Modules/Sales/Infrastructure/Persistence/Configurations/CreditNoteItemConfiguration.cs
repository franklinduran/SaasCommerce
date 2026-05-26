using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CreditNoteItemConfiguration : IEntityTypeConfiguration<CreditNoteItem>
{
  public void Configure(EntityTypeBuilder<CreditNoteItem> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("credit_note_items", "sales");
    builder.HasKey(item => item.Id);
    builder.Property(item => item.Id).ValueGeneratedNever();
    builder.Property(item => item.Quantity).HasPrecision(18, 3).IsRequired();
    builder.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
    builder.Ignore(item => item.LineTotal);
    builder.HasIndex(item => new { item.CreditNoteId, item.ProductId });
  }
}
