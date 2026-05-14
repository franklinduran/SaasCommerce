using SaasCommerce.Application.Abstractions.Auth;
using SaasCommerce.Application.Abstractions.Messaging;
using SaasCommerce.Application.Abstractions.Persistence;
using SaasCommerce.Application.Abstractions.Realtime;
using SaasCommerce.Application.Abstractions.Time;
using SaasCommerce.Infrastructure.Auth;
using SaasCommerce.Infrastructure.Messaging;
using SaasCommerce.Infrastructure.Persistence;
using SaasCommerce.Infrastructure.Realtime;
using SaasCommerce.Infrastructure.Time;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SaasCommerce.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<IBusRegistrationConfigurator>? configureMassTransit = null)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddHttpContextAccessor();
    services.AddSignalR();
    services.AddDbContext<AppDbContext>(options => ConfigureDbContext(options, configuration));

    services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    services.AddScoped<IEventBus, MassTransitEventBus>();
    services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
    services.AddScoped<ICurrentUserService, CurrentUserService>();
    services.AddSingleton<IClock, SystemClock>();

    services.AddMassTransit(configurator =>
    {
      configureMassTransit?.Invoke(configurator);

      configurator.UsingRabbitMq((context, rabbitMq) =>
      {
        var host = configuration["RabbitMq:Host"] ?? "localhost";
        var username = configuration["RabbitMq:Username"];
        var password = configuration["RabbitMq:Password"];

        rabbitMq.Host(host, "/", hostConfigurator =>
        {
          if (!string.IsNullOrWhiteSpace(username))
          {
            hostConfigurator.Username(username);
          }

          if (!string.IsNullOrWhiteSpace(password))
          {
            hostConfigurator.Password(password);
          }
        });

        rabbitMq.ConfigureEndpoints(context);
      });
    });

    return services;
  }

  private static void ConfigureDbContext(
    DbContextOptionsBuilder options,
    IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrWhiteSpace(connectionString))
    {
      options.UseNpgsql(connectionString);
    }
  }
}
