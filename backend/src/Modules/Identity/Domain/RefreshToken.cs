namespace SaasCommerce.Modules.Identity.Domain;

public sealed class RefreshToken
{
  private RefreshToken()
  {
  }

  public RefreshToken(Guid id, Guid userId, string token, DateTimeOffset expiresAt, DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(token);

    Id = id;
    UserId = userId;
    Token = token;
    ExpiresAt = expiresAt;
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public Guid UserId { get; private set; }

  public string Token { get; private set; } = string.Empty;

  public DateTimeOffset ExpiresAt { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? RevokedAt { get; private set; }

  public bool IsActive(DateTimeOffset utcNow) => RevokedAt is null && ExpiresAt > utcNow;

  public void Revoke(DateTimeOffset revokedAt) => RevokedAt = revokedAt;
}
