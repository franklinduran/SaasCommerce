using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Domain;

public sealed class BusinessPhone
{
  private BusinessPhone()
  {
  }

  public BusinessPhone(
    Guid id,
    BusinessId businessId,
    string number,
    string? label,
    bool isPrimary,
    DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(number);

    Id = id;
    BusinessId = businessId;
    Number = number.Trim();
    Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
    IsPrimary = isPrimary;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Number { get; private set; } = string.Empty;

  public string? Label { get; private set; }

  public bool IsPrimary { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }
}
