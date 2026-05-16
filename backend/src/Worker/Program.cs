using System.Globalization;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.Modules;
using SaasCommerce.Worker;
using SaasCommerce.Worker.Consumers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, loggerConfiguration) =>
  loggerConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));
builder.Services.AddModules();
builder.Services.AddBuildingBlocks(
  builder.Configuration,
  massTransit => massTransit.AddConsumer<TechnicalPingConsumer>());
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

public partial class Program
{
  protected Program()
  {
  }
}
