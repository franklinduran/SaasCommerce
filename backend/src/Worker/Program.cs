using System.Globalization;
using SaasCommerce.Application;
using SaasCommerce.Infrastructure;
using SaasCommerce.Worker;
using SaasCommerce.Worker.Consumers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
  .CreateLogger();

builder.Services.AddSerilog();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
  builder.Configuration,
  massTransit => massTransit.AddConsumer<TechnicalPingConsumer>());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

public partial class Program
{
}
