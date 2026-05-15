using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.BuildingBlocks.Infrastructure.Observability;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;
using SaasCommerce.BuildingBlocks.Infrastructure.Time;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SaasCommerce.BuildingBlocks;

public static class BuildingBlocksServiceCollectionExtensions
{
  public static IServiceCollection AddBuildingBlocks(
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
    services.AddScoped<IInboxStore, EfInboxStore>();
    services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
    services.AddScoped<ICurrentUserService, CurrentUserService>();
    services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
    services.AddSingleton<IClock, SystemClock>();

    services.AddMassTransit(configurator =>
    {
      configureMassTransit?.Invoke(configurator);

      if (configuration.GetValue<bool>("RabbitMq:UseInMemory"))
      {
        configurator.UsingInMemory((context, inMemory) =>
        {
          inMemory.ConfigureEndpoints(context);
        });

        return;
      }

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
      return;
    }

    options.UseInMemoryDatabase("SaasCommerceDevelopment");
  }
}
