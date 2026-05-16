namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record RegisterBusinessPhoneRequest(
  string Number,
  string? Label,
  bool IsPrimary);
