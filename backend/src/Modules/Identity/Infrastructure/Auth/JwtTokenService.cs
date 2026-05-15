using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SaasCommerce.Modules.Identity.Infrastructure.Auth;

public sealed class JwtTokenService(
  IConfiguration configuration,
  IClock clock) : IJwtTokenService
{
  private const string BusinessIdClaim = "business_id";
  private const string BranchIdClaim = "branch_id";

  public AccessTokenResult CreateAccessToken(User user)
  {
    ArgumentNullException.ThrowIfNull(user);

    var secret = configuration["Jwt:Secret"];

    if (string.IsNullOrWhiteSpace(secret))
    {
      throw new InvalidOperationException("Jwt:Secret must be configured before issuing tokens.");
    }

    var expiresAt = clock.UtcNow.AddMinutes(GetAccessTokenMinutes());
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var claims = CreateClaims(user);
    var token = new JwtSecurityToken(
      issuer: configuration["Jwt:Issuer"],
      audience: configuration["Jwt:Audience"],
      claims: claims,
      notBefore: clock.UtcNow.UtcDateTime,
      expires: expiresAt.UtcDateTime,
      signingCredentials: credentials);

    return new AccessTokenResult(
      new JwtSecurityTokenHandler().WriteToken(token),
      expiresAt);
  }

  private int GetAccessTokenMinutes()
  {
    var configuredMinutes = configuration.GetValue<int?>("Jwt:AccessTokenMinutes");

    return configuredMinutes is > 0
      ? configuredMinutes.Value
      : 60;
  }

  private static List<Claim> CreateClaims(User user)
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, user.Id.ToString("D")),
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString("D")),
      new(BusinessIdClaim, user.BusinessId.Value.ToString("D"))
    };

    if (user.DefaultBranchId is { } branchId)
    {
      claims.Add(new Claim(BranchIdClaim, branchId.Value.ToString("D")));
    }

    claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

    return claims;
  }
}
