using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Identity.Application.Settings;

internal static class SettingsValidation
{
  public static bool TryNormalizeBusiness(
    string businessName,
    string identificationTypeValue,
    string identificationNumberValue,
    IReadOnlyCollection<RegisterBusinessPhoneCommand>? phones,
    out BusinessIdentificationType identificationType,
    out string identificationNumber,
    out IReadOnlyCollection<BusinessPhoneInput> normalizedPhones)
  {
    identificationType = default;
    identificationNumber = string.Empty;
    normalizedPhones = [];

    if (string.IsNullOrWhiteSpace(businessName) ||
        string.IsNullOrWhiteSpace(identificationTypeValue) ||
        string.IsNullOrWhiteSpace(identificationNumberValue) ||
        !Enum.TryParse(identificationTypeValue, true, out identificationType))
    {
      return false;
    }

    identificationNumber = NormalizeIdentification(identificationType, identificationNumberValue);

    if (!IsValidIdentification(identificationType, identificationNumber))
    {
      return false;
    }

    if (!TryNormalizePhones(phones, out var normalized))
    {
      return false;
    }

    normalizedPhones = normalized;

    return true;
  }

  private static string NormalizeIdentification(
    BusinessIdentificationType identificationType,
    string identificationNumber)
  {
    var trimmed = identificationNumber.Trim();

    return identificationType is BusinessIdentificationType.Cedula or BusinessIdentificationType.Rnc
      ? KeepDigits(trimmed)
      : trimmed.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
  }

  private static bool IsValidIdentification(
    BusinessIdentificationType identificationType,
    string identificationNumber)
    => identificationType switch
    {
      BusinessIdentificationType.Cedula => identificationNumber.Length == 11,
      BusinessIdentificationType.Rnc => identificationNumber.Length == 9,
      BusinessIdentificationType.Passport => identificationNumber.Length >= 5 &&
        identificationNumber.All(char.IsLetterOrDigit),
      _ => false
    };

  private static bool TryNormalizePhones(
    IReadOnlyCollection<RegisterBusinessPhoneCommand>? phones,
    out List<BusinessPhoneInput> normalizedPhones)
  {
    normalizedPhones = [];

    if (phones is null || phones.Count == 0 || phones.Count(phone => phone.IsPrimary) != 1)
    {
      return false;
    }

    var normalized = new List<BusinessPhoneInput>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    foreach (var phone in phones)
    {
      var number = NormalizePhone(phone.Number);

      if (string.IsNullOrWhiteSpace(number) || !seen.Add(number))
      {
        return false;
      }

      normalized.Add(new BusinessPhoneInput(number, phone.Label, phone.IsPrimary));
    }

    normalizedPhones = normalized;

    return true;
  }

  private static string NormalizePhone(string? number)
    => string.IsNullOrWhiteSpace(number)
      ? string.Empty
      : KeepDigits(number);

  private static string KeepDigits(string value)
    => string.Concat(value.Where(char.IsDigit));
}
