namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record BusinessPhoneResponse(string Number, string? Label, bool IsPrimary);
