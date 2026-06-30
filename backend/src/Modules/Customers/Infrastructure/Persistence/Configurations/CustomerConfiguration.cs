using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("customers", "customers");
    builder.HasKey(customer => customer.Id);

    builder.Property(customer => customer.Id)
      .ValueGeneratedNever();

    builder.Property(customer => customer.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(customer => customer.FirstName)
      .HasMaxLength(CustomerRules.FirstNameMaxLength)
      .IsRequired();

    builder.Property(customer => customer.LastName)
      .HasMaxLength(CustomerRules.LastNameMaxLength)
      .IsRequired();

    builder.Ignore(customer => customer.FullName);

    builder.Property(customer => customer.SearchName)
      .HasMaxLength(CustomerRules.SearchNameMaxLength)
      .IsRequired();

    builder.Property(customer => customer.Phone)
      .HasMaxLength(CustomerRules.PhoneMaxLength);

    builder.Property(customer => customer.Email)
      .HasMaxLength(CustomerRules.EmailMaxLength);

    builder.Property(customer => customer.Cedula)
      .HasMaxLength(CustomerRules.CedulaMaxLength);

    builder.Property(customer => customer.IsActive)
      .IsRequired();

    builder.Property(customer => customer.CreatedAt)
      .IsRequired();

    builder.HasIndex(customer => new { customer.BusinessId, customer.SearchName });
    builder.HasIndex(customer => new { customer.BusinessId, customer.IsActive });

    // Partial unique index: a cedula must be unique per business, but only when provided.
    builder.HasIndex(customer => new { customer.BusinessId, customer.Cedula })
      .IsUnique()
      .HasFilter("\"Cedula\" IS NOT NULL")
      .HasDatabaseName("ix_customers_business_id_cedula");
  }
}
