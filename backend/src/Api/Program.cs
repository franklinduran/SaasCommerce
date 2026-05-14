using System.Text;
using System.Globalization;
using SaasCommerce.Api.Middleware;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;
using SaasCommerce.Modules;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, _, loggerConfiguration) =>
  loggerConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

builder.Services.AddModules();
builder.Services.AddBuildingBlocks(builder.Configuration);
builder.Services.AddCors(options =>
{
  options.AddPolicy(
    "Default",
    policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSaasCommerceJwt(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () =>
  Results.Ok(ApiResponse.Success("Live")));

app.MapGet("/health/ready", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
  var canConnect = await CanConnectToDatabaseAsync(dbContext, cancellationToken);

  return canConnect
    ? Results.Ok(ApiResponse.Success("Ready"))
    : Results.Json(
      ApiResponse.Failure<string>(new ApiError("DatabaseUnavailable", "PostgreSQL is not ready.")),
      statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/api/version", () =>
{
  var version = new
  {
    Name = "SaasCommerce RD",
    Api = "v1",
    Status = "BaseReady"
  };

  return Results.Ok(ApiResponse.Success<object>(version));
});

app.MapHub<BusinessHub>("/hubs/business");

app.Run();

static async Task<bool> CanConnectToDatabaseAsync(
  AppDbContext dbContext,
  CancellationToken cancellationToken)
{
  try
  {
    if (dbContext.Database.ProviderName is null)
    {
      return true;
    }

    return await dbContext.Database.CanConnectAsync(cancellationToken);
  }
  catch (InvalidOperationException)
  {
    return true;
  }
}

public partial class Program
{
}

internal static class JwtServiceCollectionExtensions
{
  public static IServiceCollection AddSaasCommerceJwt(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var secret = configuration["Jwt:Secret"];
    var issuer = configuration["Jwt:Issuer"];
    var audience = configuration["Jwt:Audience"];

    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.RequireHttpsMetadata = true;

        if (string.IsNullOrWhiteSpace(secret))
        {
          return;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
          ValidIssuer = issuer,
          ValidateAudience = !string.IsNullOrWhiteSpace(audience),
          ValidAudience = audience,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
          ValidateLifetime = true,
          ClockSkew = TimeSpan.FromMinutes(1)
        };
      });

    return services;
  }
}
