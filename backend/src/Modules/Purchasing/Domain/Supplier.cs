using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Domain;

public sealed record SupplierContactInfo(string? Rnc, string? Phone, string? Email, string? Address);

public sealed class Supplier
{
  private Supplier()
  {
  }

  public Supplier(
    Guid id,
    BusinessId businessId,
    string name,
    SupplierContactInfo contact,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Supplier id is required.", nameof(id));
    }

    Validate(name, contact?.Email);

    Id = id;
    BusinessId = businessId;
    IsActive = true;
    CreatedAt = createdAt;
    ApplyDetails(name, contact?.Rnc, contact?.Phone, contact?.Email, contact?.Address);
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public string SearchName { get; private set; } = string.Empty;

  public string? Rnc { get; private set; }

  public string? Phone { get; private set; }

  public string? Email { get; private set; }

  public string? Address { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public void Update(
    string name,
    string? rnc,
    string? phone,
    string? email,
    string? address,
    bool isActive,
    DateTimeOffset updatedAt)
  {
    Validate(name, email);
    ApplyDetails(name, rnc, phone, email, address);
    IsActive = isActive;
    UpdatedAt = updatedAt;
  }

  private void ApplyDetails(
    string name,
    string? rnc,
    string? phone,
    string? email,
    string? address)
  {
    Name = name.Trim();
    SearchName = Name.ToUpperInvariant();
    Rnc = NormalizeDigits(rnc);
    Phone = NormalizeDigits(phone);
    Email = NormalizeEmail(email);
    Address = NormalizeOptional(address);
  }

  private static void Validate(string name, string? email)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    if (name.Trim().Length > SupplierRules.NameMaxLength)
    {
      throw new ArgumentOutOfRangeException(nameof(name), "Supplier name is too long.");
    }

    var normalizedEmail = NormalizeEmail(email);

    if (normalizedEmail is not null &&
        (normalizedEmail.Length > SupplierRules.EmailMaxLength ||
         !normalizedEmail.Contains('@', StringComparison.Ordinal) ||
         normalizedEmail.StartsWith('@') ||
         normalizedEmail.EndsWith('@')))
    {
      throw new ArgumentException("Supplier email is invalid.", nameof(email));
    }
  }

  private static string? NormalizeDigits(string? value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    var digits = new string(value.Where(char.IsDigit).ToArray());

    return string.IsNullOrWhiteSpace(digits) ? null : digits;
  }

  private static string? NormalizeEmail(string? email)
    => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class SupplierRules
{
  public const int NameMaxLength = 180;
  public const int SearchNameMaxLength = 180;
  public const int RncMaxLength = 20;
  public const int PhoneMaxLength = 20;
  public const int EmailMaxLength = 320;
  public const int AddressMaxLength = 400;
}
