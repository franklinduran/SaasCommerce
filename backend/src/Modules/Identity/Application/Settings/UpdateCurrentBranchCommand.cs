namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed record UpdateCurrentBranchCommand(string Name, string? Address, string? Phone);
