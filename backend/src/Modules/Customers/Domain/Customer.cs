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
    string firstName,
    string lastName,
    string? phone,
    string? email,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Customer id is required.", nameof(id));
    }

    Validate(firstName, lastName, phone, email);

    Id = id;
    BusinessId = businessId;
    CreatedAt = createdAt;
    IsActive = true;
    ApplyDetails(firstName, lastName, phone, email);
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string FirstName { get; private set; } = string.Empty;

  public string LastName { get; private set; } = string.Empty;

  public string FullName => $"{FirstName} {LastName}".Trim();

  public string SearchName { get; private set; } = string.Empty;

  public string? Phone { get; private set; }

  public string? Email { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public DateTimeOffset? DeactivatedAt { get; private set; }

  public void Update(
    string firstName,
    string lastName,
    string? phone,
    string? email,
    bool isActive,
    DateTimeOffset updatedAt)
  {
    Validate(firstName, lastName, phone, email);
    ApplyDetails(firstName, lastName, phone, email);

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

  private void ApplyDetails(string firstName, string lastName, string? phone, string? email)
  {
    FirstName = firstName.Trim();
    LastName = lastName.Trim();
    SearchName = FullName.ToUpperInvariant();
    Phone = NormalizePhone(phone);
    Email = NormalizeEmail(email);
  }

  private static void Validate(string firstName, string lastName, string? phone, string? email)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
    ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

    if (firstName.Trim().Length > CustomerRules.FirstNameMaxLength)
    {
      throw new ArgumentOutOfRangeException(nameof(firstName), "First name is too long.");
    }

    if (lastName.Trim().Length > CustomerRules.LastNameMaxLength)
    {
      throw new ArgumentOutOfRangeException(nameof(lastName), "Last name is too long.");
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
  public const int FirstNameMaxLength = 80;
  public const int LastNameMaxLength = 80;
  public const int SearchNameMaxLength = 162; // FirstName + space + LastName
  public const int PhoneMaxLength = 20;
  public const int PhoneMinLength = 7;
  public const int EmailMaxLength = 320;
}
