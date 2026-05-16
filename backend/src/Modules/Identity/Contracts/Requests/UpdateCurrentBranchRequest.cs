namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record UpdateCurrentBranchRequest(string Name, string? Address, string? Phone);
