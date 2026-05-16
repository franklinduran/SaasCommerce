namespace SaasCommerce.Modules.Identity.Application.Account;

public sealed record RegisterBusinessPhoneCommand(
  string Number,
  string? Label,
  bool IsPrimary);
