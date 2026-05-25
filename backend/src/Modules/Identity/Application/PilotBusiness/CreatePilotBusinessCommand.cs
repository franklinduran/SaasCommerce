namespace SaasCommerce.Modules.Identity.Application.PilotBusiness;

/// <summary>
/// Creates a complete pilot business with its admin user and main branch.
/// Only callable by a SaaS platform administrator.
/// </summary>
public sealed record CreatePilotBusinessCommand(
  string BusinessName,
  string IdentificationType,
  string IdentificationNumber,
  string Phone,
  string BranchName,
  string AdminFullName,
  string AdminEmail,
  string AdminPassword);
