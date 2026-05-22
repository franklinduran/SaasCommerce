using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Domain;

public sealed class Business
{
  private readonly List<Branch> branches = [];
  private readonly List<BusinessPhone> phones = [];

  private Business()
  {
  }

  public Business(
    BusinessId id,
    string name,
    DateTimeOffset createdAt,
    BusinessIdentificationType? identificationType = null,
    string? identificationNumber = null)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    Name = name.Trim();
    IdentificationType = identificationType;
    IdentificationNumber = NormalizeOptional(identificationNumber);
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
    IsActive = true;
  }

  public BusinessId Id { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public BusinessIdentificationType? IdentificationType { get; private set; }

  public string? IdentificationNumber { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public IReadOnlyCollection<Branch> Branches => branches.AsReadOnly();

  public IReadOnlyCollection<BusinessPhone> Phones => phones.AsReadOnly();

  public Branch AddBranch(BranchId branchId, string name, string code, DateTimeOffset createdAt, bool isMain = false)
  {
    var branch = new Branch(branchId, Id, name, code, createdAt, isMain);

    branches.Add(branch);

    return branch;
  }

  public BusinessPhone AddPhone(
    Guid phoneId,
    string number,
    string? label,
    bool isPrimary,
    DateTimeOffset createdAt)
  {
    var phone = new BusinessPhone(phoneId, Id, number, label, isPrimary, createdAt);

    phones.Add(phone);

    return phone;
  }

  public void UpdateDetails(
    string name,
    BusinessIdentificationType identificationType,
    string identificationNumber,
    DateTimeOffset updatedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(identificationNumber);

    Name = name.Trim();
    IdentificationType = identificationType;
    IdentificationNumber = identificationNumber.Trim();
    UpdatedAt = updatedAt;
  }

  public void ReplacePhones(
    IReadOnlyCollection<BusinessPhoneInput> newPhones,
    DateTimeOffset updatedAt)
  {
    ArgumentNullException.ThrowIfNull(newPhones);

    phones.Clear();

    foreach (var phone in newPhones)
    {
      phones.Add(new BusinessPhone(
        Guid.NewGuid(),
        Id,
        phone.Number,
        phone.Label,
        phone.IsPrimary,
        updatedAt));
    }

    UpdatedAt = updatedAt;
  }

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record BusinessPhoneInput(string Number, string? Label, bool IsPrimary);
