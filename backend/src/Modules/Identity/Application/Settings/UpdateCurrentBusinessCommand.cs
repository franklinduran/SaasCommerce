using SaasCommerce.Modules.Identity.Application.Account;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed record UpdateCurrentBusinessCommand(
  string BusinessName,
  string IdentificationType,
  string IdentificationNumber,
  IReadOnlyCollection<RegisterBusinessPhoneCommand> Phones);
