using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Infrastructure.Persistence.Configurations;

public sealed class CustomerCreditAccountConfiguration : IEntityTypeConfiguration<CustomerCreditAccount>
{
  public void Configure(EntityTypeBuilder<CustomerCreditAccount> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable("customer_credit_accounts", "customers");
    builder.HasKey(account => account.Id);

    builder.Property(account => account.Id)
      .ValueGeneratedNever();

    builder.Property(account => account.BusinessId)
      .HasConversion(id => id.Value, value => new BusinessId(value))
      .IsRequired();

    builder.Property(account => account.CustomerId)
      .IsRequired();

    builder.Property(account => account.CreditLimit)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(account => account.CurrentBalance)
      .HasPrecision(18, 2)
      .IsRequired();

    builder.Property(account => account.Status)
      .HasConversion<string>()
      .HasMaxLength(CustomerCreditRules.StatusMaxLength)
      .IsRequired();

    builder.Property(account => account.CreatedAt)
      .IsRequired();

    builder.Property(account => account.UpdatedAt)
      .IsRequired();

    builder.HasIndex(account => new { account.BusinessId, account.CustomerId })
      .IsUnique();
  }
}
