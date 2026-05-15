using System.Text;
using System.Globalization;
using SaasCommerce.Api.Middleware;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;
using SaasCommerce.Modules;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.SharedKernel;
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
builder.Services.AddSaasCommerceJwt(builder.Configuration, builder.Environment);
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

app.MapPost(
  "/api/auth/login",
  async (
    LoginRequest request,
    LoginHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new LoginCommand(request.Email, request.Password),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status401Unauthorized);
  })
  .AllowAnonymous();

app.MapPost(
  "/api/auth/refresh",
  async (
    RefreshTokenRequest request,
    RefreshTokenHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new RefreshTokenCommand(request.RefreshToken),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status401Unauthorized);
  })
  .AllowAnonymous();

app.MapGet(
  "/api/me",
  async (
    GetCurrentUserHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status401Unauthorized);
  })
  .RequireAuthorization();

app.MapHub<BusinessHub>("/hubs/business");

if (app.Environment.IsDevelopment())
{
  await MigrateDatabaseAsync(app.Services);
  await app.Services.SeedDevelopmentDataAsync();
}

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

static IResult ToApiResult<T>(
  Result<T> result,
  ICorrelationIdProvider correlationIdProvider,
  int failureStatusCode = StatusCodes.Status400BadRequest)
{
  ArgumentNullException.ThrowIfNull(result);
  ArgumentNullException.ThrowIfNull(correlationIdProvider);

  return result.IsSuccess
    ? Results.Ok(ApiResponse.Success(result.Value, correlationIdProvider.CorrelationId))
    : Results.Json(
      ApiResponse.Failure<T>(ToApiError(result.Error), correlationIdProvider.CorrelationId),
      statusCode: failureStatusCode);
}

static ApiError ToApiError(DomainError error)
  => new(error.Code, error.Message);

static async Task MigrateDatabaseAsync(
  IServiceProvider serviceProvider,
  CancellationToken cancellationToken = default)
{
  ArgumentNullException.ThrowIfNull(serviceProvider);

  using var scope = serviceProvider.CreateScope();
  var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

  if (dbContext.Database.IsRelational())
  {
    await dbContext.Database.MigrateAsync(cancellationToken);
  }
}

public partial class Program
{
}

internal static class JwtServiceCollectionExtensions
{
  public static IServiceCollection AddSaasCommerceJwt(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(environment);

    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer();

    services
      .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
      .Configure<IConfiguration, IHostEnvironment>((options, currentConfiguration, currentEnvironment) =>
      {
        var secret = currentConfiguration["Jwt:Secret"];
        var issuer = currentConfiguration["Jwt:Issuer"];
        var audience = currentConfiguration["Jwt:Audience"];

        options.RequireHttpsMetadata = !currentEnvironment.IsDevelopment();
        options.IncludeErrorDetails = currentEnvironment.IsDevelopment();
        options.Events = new JwtBearerEvents
        {
          OnChallenge = async context =>
          {
            context.HandleResponse();

            var correlationIdProvider = context.HttpContext.RequestServices
              .GetService<ICorrelationIdProvider>();
            var correlationId = correlationIdProvider?.CorrelationId ??
              context.HttpContext.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(
              ApiResponse.Failure<object?>(
                new ApiError("unauthorized", "Authentication is required."),
                correlationId));
          }
        };

        if (!string.IsNullOrWhiteSpace(secret))
        {
          options.MapInboundClaims = false;
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
        }
      });

    return services;
  }
}
