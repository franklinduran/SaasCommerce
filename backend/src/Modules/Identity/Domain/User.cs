using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Domain;

public sealed class User
{
  private readonly List<Role> roles = [];
  private readonly List<RefreshToken> refreshTokens = [];

  private User()
  {
  }

  public User(
    Guid id,
    BusinessId businessId,
    BranchId? defaultBranchId,
    string fullName,
    string email,
    string passwordHash,
    DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
    ArgumentException.ThrowIfNullOrWhiteSpace(email);
    ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

    Id = id;
    BusinessId = businessId;
    DefaultBranchId = defaultBranchId;
    FullName = fullName.Trim();
    Email = email.Trim().ToLowerInvariant();
    PasswordHash = passwordHash;
    CreatedAt = createdAt;
    IsActive = true;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId? DefaultBranchId { get; private set; }

  public string FullName { get; private set; } = string.Empty;

  public string Email { get; private set; } = string.Empty;

  public string PasswordHash { get; private set; } = string.Empty;

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public IReadOnlyCollection<Role> Roles => roles.AsReadOnly();

  public IReadOnlyCollection<RefreshToken> RefreshTokens => refreshTokens.AsReadOnly();

  public void AddRole(Role role)
  {
    ArgumentNullException.ThrowIfNull(role);

    if (roles.All(current => current.Id != role.Id))
    {
      roles.Add(role);
    }
  }

  public RefreshToken AddRefreshToken(
    string token,
    DateTimeOffset expiresAt,
    DateTimeOffset createdAt)
  {
    var refreshToken = new RefreshToken(Guid.NewGuid(), Id, token, expiresAt, createdAt);

    refreshTokens.Add(refreshToken);

    return refreshToken;
  }
}
