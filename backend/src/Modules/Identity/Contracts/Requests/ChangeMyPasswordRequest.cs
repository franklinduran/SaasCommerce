namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record ChangeMyPasswordRequest(string CurrentPassword, string NewPassword);
