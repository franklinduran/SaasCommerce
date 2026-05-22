using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryTransferConfiguration : IEntityTypeConfiguration<InventoryTransfer>
{
  public void Configure(EntityTypeBuilder<InventoryTransfer> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("inventory_transfers", "inventory");

    builder.HasKey(transfer => transfer.Id);

    builder.Property(transfer => transfer.Id)
      .ValueGeneratedNever();

    builder.Property(transfer => transfer.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(transfer => transfer.SourceBranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(transfer => transfer.TargetBranchId)
      .HasConversion(id => id.Value, value => new BranchId(value))
      .IsRequired();

    builder.Property(transfer => transfer.CreatedByUserId)
      .IsRequired();

    builder.Property(transfer => transfer.Status)
      .HasConversion<string>()
      .HasMaxLength(32)
      .IsRequired();

    builder.Property(transfer => transfer.Note)
      .HasMaxLength(500);

    builder.Property(transfer => transfer.FailureReason)
      .HasMaxLength(1000);

    builder.Property(transfer => transfer.CreatedAt)
      .IsRequired();

    builder.Property(transfer => transfer.UpdatedAt)
      .IsRequired();

    builder.HasMany(transfer => transfer.Items)
      .WithOne()
      .HasForeignKey(item => item.TransferId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(transfer => transfer.BusinessId);
    builder.HasIndex(transfer => new { transfer.BusinessId, transfer.SourceBranchId });
    builder.HasIndex(transfer => new { transfer.BusinessId, transfer.TargetBranchId });
    builder.HasIndex(transfer => new { transfer.BusinessId, transfer.Status });
    builder.HasIndex(transfer => new { transfer.BusinessId, transfer.CreatedAt });
  }
}
