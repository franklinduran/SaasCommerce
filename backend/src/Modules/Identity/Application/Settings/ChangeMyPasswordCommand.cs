namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword);
