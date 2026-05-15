using System.Security.Cryptography;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using Microsoft.AspNetCore.WebUtilities;

namespace SaasCommerce.Modules.Identity.Infrastructure.Auth;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
  public string Create()
    => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
}
