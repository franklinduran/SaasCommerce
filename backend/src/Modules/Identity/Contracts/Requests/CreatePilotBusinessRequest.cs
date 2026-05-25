namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record CreatePilotBusinessRequest(
  string BusinessName,
  string IdentificationType,
  string IdentificationNumber,
  string Phone,
  string BranchName,
  string AdminFullName,
  string AdminEmail,
  string AdminPassword);
