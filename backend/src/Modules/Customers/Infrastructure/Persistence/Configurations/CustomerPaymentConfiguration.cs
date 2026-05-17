using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Infrastructure.Persistence.Configurations;

public sealed class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
{
  public void Configure(EntityTypeBuilder<CustomerPayment> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("customer_payments", "customers");
    builder.HasKey(payment => payment.Id);

    builder.Property(payment => payment.Id)
      .ValueGeneratedNever();

    builder.Property(payment => payment.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(payment => payment.CustomerId)
      .IsRequired();

    builder.Property(payment => payment.Amount)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(payment => payment.Note)
      .HasMaxLength(CustomerCreditRules.NoteMaxLength);

    builder.Property(payment => payment.CreatedAt)
      .IsRequired();

    builder.HasIndex(payment => new { payment.BusinessId, payment.CustomerId, payment.CreatedAt });
  }
}
