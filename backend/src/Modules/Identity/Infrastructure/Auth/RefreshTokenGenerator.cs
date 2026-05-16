using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using SaasCommerce.Modules.Identity.Application.Abstractions;

namespace SaasCommerce.Modules.Identity.Infrastructure.Auth;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
  public string Create()
    => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
}
