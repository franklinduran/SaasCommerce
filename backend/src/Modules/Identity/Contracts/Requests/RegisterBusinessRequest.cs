namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record RegisterBusinessRequest(
  string BusinessName,
  string OwnerFullName,
  string Email,
  string Password,
  string? IdentificationType,
  string? IdentificationNumber,
  IReadOnlyCollection<RegisterBusinessPhoneRequest>? Phones,
  string BranchName);
