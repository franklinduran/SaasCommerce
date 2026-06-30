using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Observability;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;
using SaasCommerce.BuildingBlocks.Infrastructure.Time;

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
    services.AddSingleton<OutboxTrigger>();
    services.AddSingleton<OutboxTriggerInterceptor>();
    services.AddSingleton<OutboxTransactionInterceptor>();
    services.AddDbContext<AppDbContext>((sp, options) =>
    {
      ConfigureDbContext(options, configuration);
      options.AddInterceptors(
        sp.GetRequiredService<OutboxTriggerInterceptor>(),
        sp.GetRequiredService<OutboxTransactionInterceptor>());
    });
    services.Configure<OutboxPublisherOptions>(configuration.GetSection("OutboxPublisher"));
    services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    services.AddScoped<IEventBus, MassTransitEventBus>();
    services.AddScoped<IInboxStore, EfInboxStore>();
    services.AddScoped<IOutboxWriter, EfOutboxWriter>();
    services.AddScoped<OutboxPublisher>();
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

        // Retry transient PostgreSQL errors before sending to the error queue.
        // 40001 = serialization_failure (concurrent saga inserts), 40P01 = deadlock.
        // These are ephemeral — a second attempt succeeds once the contention resolves.
        rabbitMq.UseMessageRetry(retry => retry
          .Intervals(
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3))
          .Handle<Npgsql.PostgresException>(ex => ex.SqlState is "40001" or "40P01"));

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

    var inMemoryDatabaseName = configuration["Database:InMemoryName"] ?? "SaasCommerceDevelopment";

    options.UseInMemoryDatabase(inMemoryDatabaseName);
  }
}
