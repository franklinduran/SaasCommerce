namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record ResetUserPasswordCommand(Guid TargetUserId);
