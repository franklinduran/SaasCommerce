namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record CurrentBusinessResponse(
  Guid BusinessId,
  string Name,
  string? IdentificationType,
  string? IdentificationNumber,
  IReadOnlyCollection<BusinessPhoneResponse> Phones);
