namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record UpdateCurrentBusinessRequest(
  string BusinessName,
  string IdentificationType,
  string IdentificationNumber,
  IReadOnlyCollection<RegisterBusinessPhoneRequest> Phones);
