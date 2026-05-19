namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record CreateUserCommand(
  string FullName,
  string Email,
  string Password,
  string Role,
  Guid? DefaultBranchId);
