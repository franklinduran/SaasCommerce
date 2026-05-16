namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record UpdateMyProfileRequest(string FullName, string? Phone);
