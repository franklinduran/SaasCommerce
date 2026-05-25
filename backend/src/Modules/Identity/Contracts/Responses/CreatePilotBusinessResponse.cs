namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record CreatePilotBusinessResponse(
  Guid BusinessId,
  Guid BranchId,
  Guid AdminUserId,
  string BusinessName,
  string BranchName,
  string AdminEmail,
  DateTimeOffset? TrialEndsAt);
