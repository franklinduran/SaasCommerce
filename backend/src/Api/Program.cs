using System.Text;
using System.Globalization;
using SaasCommerce.Api.Middleware;
using SaasCommerce.Application;
using SaasCommerce.Contracts.Common;
using SaasCommerce.Infrastructure;
using SaasCommerce.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, _, loggerConfiguration) =>
  loggerConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
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

app.MapGet("/health/ready", () =>
  Results.Ok(ApiResponse.Success("Ready")));

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
