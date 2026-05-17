using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Domain;

public sealed class Customer
{
  private Customer()
  {
  }

  public Customer(
    Guid id,
    BusinessId businessId,
    string fullName,
    string? phone,
    string? email,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Customer id is required.", nameof(id));
    }

    Validate(fullName, phone, email);

    Id = id;
    BusinessId = businessId;
    CreatedAt = createdAt;
    IsActive = true;
    ApplyDetails(fullName, phone, email);
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string FullName { get; private set; } = string.Empty;

  public string SearchName { get; private set; } = string.Empty;

  public string? Phone { get; private set; }

  public string? Email { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public DateTimeOffset? DeactivatedAt { get; private set; }

  public void Update(
    string fullName,
    string? phone,
    string? email,
    bool isActive,
    DateTimeOffset updatedAt)
  {
    Validate(fullName, phone, email);
    ApplyDetails(fullName, phone, email);

    if (isActive)
    {
      IsActive = true;
      DeactivatedAt = null;
    }
    else
    {
      Deactivate(updatedAt);
    }

    UpdatedAt = updatedAt;
  }

  public void Deactivate(DateTimeOffset deactivatedAt)
  {
    if (!IsActive)
    {
      return;
    }

    IsActive = false;
    DeactivatedAt = deactivatedAt;
    UpdatedAt = deactivatedAt;
  }

  private void ApplyDetails(string fullName, string? phone, string? email)
  {
    FullName = fullName.Trim();
    SearchName = FullName.ToUpperInvariant();
    Phone = NormalizePhone(phone);
    Email = NormalizeEmail(email);
  }

  private static void Validate(string fullName, string? phone, string? email)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

    if (fullName.Trim().Length > CustomerRules.FullNameMaxLength)
    {
      throw new ArgumentOutOfRangeException(nameof(fullName), "Customer name is too long.");
    }

    var normalizedPhone = NormalizePhone(phone);

    if (normalizedPhone is not null &&
        (normalizedPhone.Length < CustomerRules.PhoneMinLength ||
         normalizedPhone.Length > CustomerRules.PhoneMaxLength))
    {
      throw new ArgumentException("Customer phone is invalid.", nameof(phone));
    }

    var normalizedEmail = NormalizeEmail(email);

    if (normalizedEmail is not null &&
        (normalizedEmail.Length > CustomerRules.EmailMaxLength ||
         !normalizedEmail.Contains('@', StringComparison.Ordinal) ||
         normalizedEmail.StartsWith('@') ||
         normalizedEmail.EndsWith('@')))
    {
      throw new ArgumentException("Customer email is invalid.", nameof(email));
    }
  }

  private static string? NormalizePhone(string? phone)
  {
    if (string.IsNullOrWhiteSpace(phone))
    {
      return null;
    }

    var digits = new string(phone.Where(char.IsDigit).ToArray());

    return string.IsNullOrWhiteSpace(digits) ? null : digits;
  }

  private static string? NormalizeEmail(string? email)
    => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}

public static class CustomerRules
{
  public const int FullNameMaxLength = 160;
  public const int SearchNameMaxLength = 160;
  public const int PhoneMaxLength = 20;
  public const int PhoneMinLength = 7;
  public const int EmailMaxLength = 320;
}
