namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record ActivateUserCommand(Guid TargetUserId);
