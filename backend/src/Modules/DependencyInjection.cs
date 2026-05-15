using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Development;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;

namespace SaasCommerce.Modules;

public static class ModulesServiceCollectionExtensions
{
  public static IServiceCollection AddModules(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddValidatorsFromAssembly(ModulesAssemblyReference.Assembly);
    services.AddScoped<IIdentityUserRepository, EfIdentityUserRepository>();
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
    services.AddScoped<LoginHandler>();
    services.AddScoped<RefreshTokenHandler>();
    services.AddScoped<GetCurrentUserHandler>();
    services.AddScoped<DevelopmentDataSeeder>();

    return services;
  }
}
