using System.Security.Cryptography;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using Microsoft.AspNetCore.WebUtilities;

namespace SaasCommerce.Modules.Identity.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
  private const int SaltSize = 16;
  private const int HashSize = 32;
  private const int Iterations = 100_000;
  private const char Separator = '.';

  public string Hash(string password)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(password);

    var salt = RandomNumberGenerator.GetBytes(SaltSize);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
      password,
      salt,
      Iterations,
      HashAlgorithmName.SHA256,
      HashSize);

    return string.Join(
      Separator,
      "v1",
      Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture),
      WebEncoders.Base64UrlEncode(salt),
      WebEncoders.Base64UrlEncode(hash));
  }

  public bool Verify(string password, string passwordHash)
  {
    if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
    {
      return false;
    }

    var parts = passwordHash.Split(Separator);

    if (parts.Length != 4 ||
        parts[0] != "v1" ||
        !int.TryParse(parts[1], out var iterations))
    {
      return false;
    }

    var salt = WebEncoders.Base64UrlDecode(parts[2]);
    var expectedHash = WebEncoders.Base64UrlDecode(parts[3]);
    var actualHash = Rfc2898DeriveBytes.Pbkdf2(
      password,
      salt,
      iterations,
      HashAlgorithmName.SHA256,
      expectedHash.Length);

    return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
  }
}
