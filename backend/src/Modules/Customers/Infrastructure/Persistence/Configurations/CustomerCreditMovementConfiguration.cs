using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Infrastructure.Persistence.Configurations;

public sealed class CustomerCreditMovementConfiguration : IEntityTypeConfiguration<CustomerCreditMovement>
{
  public void Configure(EntityTypeBuilder<CustomerCreditMovement> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("customer_credit_movements", "customers");
    builder.HasKey(movement => movement.Id);

    builder.Property(movement => movement.Id)
      .ValueGeneratedNever();

    builder.Property(movement => movement.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(movement => movement.CustomerId)
      .IsRequired();

    builder.Property(movement => movement.Type)
      .HasConversion<string>()
      .HasMaxLength(CustomerCreditRules.MovementTypeMaxLength)
      .IsRequired();

    builder.Property(movement => movement.Amount)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(movement => movement.PreviousBalance)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(movement => movement.NewBalance)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(movement => movement.Note)
      .HasMaxLength(CustomerCreditRules.NoteMaxLength);

    builder.Property(movement => movement.CreatedAt)
      .IsRequired();

    builder.HasIndex(movement => new { movement.BusinessId, movement.CustomerId, movement.CreatedAt });
    builder.HasIndex(movement => new { movement.BusinessId, movement.SaleId })
      .IsUnique()
      .HasFilter("\"SaleId\" IS NOT NULL");
    builder.HasIndex(movement => new { movement.BusinessId, movement.PaymentId })
      .IsUnique()
      .HasFilter("\"PaymentId\" IS NOT NULL");
  }
}
