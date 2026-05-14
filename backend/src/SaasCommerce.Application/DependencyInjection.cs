using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SaasCommerce.Application;

public static class ApplicationServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddValidatorsFromAssembly(ApplicationAssemblyReference.Assembly);

    return services;
  }
}
