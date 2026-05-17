using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Sagas.Sales;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Configurations;

public sealed class SaleSagaStateConfiguration : IEntityTypeConfiguration<SaleSagaState>
{
  public void Configure(EntityTypeBuilder<SaleSagaState> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("sale_saga_states");
    builder.HasKey(state => state.CorrelationId);

    builder.Property(state => state.SaleId).IsRequired();
    builder.Property(state => state.BusinessId).IsRequired();
    builder.Property(state => state.BranchId).IsRequired();
    builder.Property(state => state.UserId).IsRequired();
    builder.Property(state => state.CurrentState).HasMaxLength(64).IsRequired();
    builder.Property(state => state.CreatedAt).IsRequired();
    builder.Property(state => state.UpdatedAt).IsRequired();
    builder.Property(state => state.CompletedAt);
    builder.Property(state => state.FailedAt);
    builder.Property(state => state.FailureReason).HasMaxLength(1_000);

    builder.HasIndex(state => state.SaleId).IsUnique();
    builder.HasIndex(state => new { state.BusinessId, state.CurrentState });
  }
}
