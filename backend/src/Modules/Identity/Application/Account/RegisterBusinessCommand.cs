namespace SaasCommerce.Modules.Identity.Application.Account;

public sealed record RegisterBusinessCommand(
  string BusinessName,
  string OwnerFullName,
  string Email,
  string Password,
  string? IdentificationType,
  string? IdentificationNumber,
  IReadOnlyCollection<RegisterBusinessPhoneCommand>? Phones,
  string BranchName,
  Guid? PlanId = null);
