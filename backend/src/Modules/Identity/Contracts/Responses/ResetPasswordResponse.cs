namespace SaasCommerce.Modules.Identity.Contracts.Responses;

/// <summary>
/// Temporary password generated when resetting a user's password.
/// This password is returned ONCE and the user must change it on next login.
/// The password is NOT stored — only the hash of this temporary password is persisted.
/// </summary>
public sealed record ResetPasswordResponse(
  string TemporaryPassword);
