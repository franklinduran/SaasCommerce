using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SaasCommerce.Api;
using SaasCommerce.Api.Endpoints;
using SaasCommerce.Api.Middleware;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;
using SaasCommerce.Modules;
using SaasCommerce.Modules.Catalog.Application.Categories;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Requests;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Requests;
using SaasCommerce.SharedKernel;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
const string accountTag = "Account";
const string authTag = "Auth";
const string catalogTag = "Catalog";
const string identityTag = "Identity";
const string inventoryTag = "Inventory";
const string realtimeTag = "Realtime";
const string systemTag = "System";
const string tenancyTag = "Tenancy";

builder.Host.UseSerilog((_, _, loggerConfiguration) =>
  loggerConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

builder.Services.AddModules();
builder.Services.AddBuildingBlocks(builder.Configuration);
builder.Services.AddCors(options =>
{
  var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

  if (allowedOrigins.Length == 0)
  {
    allowedOrigins = ["http://localhost:5173", "http://127.0.0.1:5173"];
  }

  options.AddPolicy(
    "Default",
    policy => policy
      .WithOrigins(allowedOrigins)
      .AllowAnyHeader()
      .AllowAnyMethod());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc(
    "v1",
    new OpenApiInfo
    {
      Title = "SaasCommerce API",
      Version = "v1",
      Description = "API modular para comercio, identidad, catalogo e inventario."
    });

  options.AddSecurityDefinition(
    "Bearer",
    new OpenApiSecurityScheme
    {
      Name = "Authorization",
      Type = SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT",
      In = ParameterLocation.Header,
      Description = "Pega el JWT sin escribir Bearer. Swagger agregara el prefijo automaticamente."
    });

  options.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecuritySchemeReference("Bearer", openApiDocument, null),
      []
    }
  });
});
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
  Results.Ok(ApiResponse.Success("Live")))
  .WithTags(systemTag);

app.MapGet("/health/ready", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
  var canConnect = await CanConnectToDatabaseAsync(dbContext, cancellationToken);

  return canConnect
    ? Results.Ok(ApiResponse.Success("Ready"))
    : Results.Json(
      ApiResponse.Failure<string>(new ApiError("DatabaseUnavailable", "PostgreSQL is not ready.")),
      statusCode: StatusCodes.Status503ServiceUnavailable);
})
  .WithTags(systemTag);

app.MapGet("/api/version", () =>
{
  var version = new
  {
    Name = "SaasCommerce RD",
    Api = "v1",
    Status = "BaseReady"
  };

  return Results.Ok(ApiResponse.Success<object>(version));
})
  .WithTags(systemTag);

app.MapPost(
  "/api/account/register-business",
  async (
    RegisterBusinessRequest request,
    RegisterBusinessHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new RegisterBusinessCommand(
        request.BusinessName,
        request.OwnerFullName,
        request.Email,
        request.Password,
        request.IdentificationType,
        request.IdentificationNumber,
        request.Phones?
          .Select(phone => new RegisterBusinessPhoneCommand(phone.Number, phone.Label, phone.IsPrimary))
          .ToArray(),
        request.BranchName),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .AllowAnonymous()
  .WithTags(accountTag);

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

    return ToApiResult(result, correlationIdProvider);
  })
  .AllowAnonymous()
  .WithTags(authTag);

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
  .AllowAnonymous()
  .WithTags(authTag);

app.MapGet(
  "/api/me",
  async (
    GetMeHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(identityTag);

app.MapPut(
  "/api/me/profile",
  async (
    UpdateMyProfileRequest request,
    UpdateMyProfileHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new UpdateMyProfileCommand(request.FullName, request.Phone),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status401Unauthorized);
  })
  .RequireAuthorization()
  .WithTags(identityTag);

app.MapPut(
  "/api/me/password",
  async (
    ChangeMyPasswordRequest request,
    ChangeMyPasswordHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new ChangeMyPasswordCommand(request.CurrentPassword, request.NewPassword),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status400BadRequest);
  })
  .RequireAuthorization()
  .WithTags(identityTag);

app.MapGet(
  "/api/business/current",
  async (
    GetCurrentBusinessHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status401Unauthorized);
  })
  .RequireAuthorization()
  .WithTags(tenancyTag);

app.MapPut(
  "/api/business/current",
  async (
    UpdateCurrentBusinessRequest request,
    UpdateCurrentBusinessHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new UpdateCurrentBusinessCommand(
        request.BusinessName,
        request.IdentificationType,
        request.IdentificationNumber,
        request.Phones
          .Select(phone => new RegisterBusinessPhoneCommand(phone.Number, phone.Label, phone.IsPrimary))
          .ToArray()),
      cancellationToken);
    var failureStatusCode = result.IsFailure && result.Error.Code == "forbidden"
      ? StatusCodes.Status403Forbidden
      : StatusCodes.Status400BadRequest;

    return ToApiResult(result, correlationIdProvider, failureStatusCode);
  })
  .RequireAuthorization()
  .WithTags(tenancyTag);

app.MapGet(
  "/api/branches/current",
  async (
    GetCurrentBranchHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status401Unauthorized);
  })
  .RequireAuthorization()
  .WithTags(tenancyTag);

app.MapPut(
  "/api/branches/current",
  async (
    UpdateCurrentBranchRequest request,
    UpdateCurrentBranchHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new UpdateCurrentBranchCommand(request.Name, request.Address, request.Phone),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(tenancyTag);

app.MapPost(
  "/api/catalog/products",
  async (
    CreateProductRequest request,
    CreateProductHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new CreateProductCommand(
        request.ProductType,
        request.Name,
        request.Description,
        request.Sku,
        request.Barcode,
        request.CategoryId,
        request.BrandId,
        request.UnitOfMeasure,
        request.SalePrice,
        request.CostPrice,
        request.WholesalePrice,
        request.MinSalePrice,
        request.TaxCategory,
        request.TaxRate,
        request.IsTaxIncluded,
        request.AllowsDiscount,
        request.TrackInventory,
        request.MinimumStock,
        request.MaximumStock,
        request.ReorderPoint,
        request.AllowNegativeStock,
        request.InternalCode,
        request.SupplierCode,
        request.ParentProductId,
        request.VariantName,
        request.AttributesJson),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapPut(
  "/api/catalog/products/{id:guid}",
  async (
    Guid id,
    UpdateProductRequest request,
    UpdateProductHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new UpdateProductCommand(
        id,
        request.ProductType,
        request.Name,
        request.Description,
        request.Sku,
        request.Barcode,
        request.CategoryId,
        request.BrandId,
        request.UnitOfMeasure,
        request.SalePrice,
        request.CostPrice,
        request.WholesalePrice,
        request.MinSalePrice,
        request.TaxCategory,
        request.TaxRate,
        request.IsTaxIncluded,
        request.AllowsDiscount,
        request.TrackInventory,
        request.MinimumStock,
        request.MaximumStock,
        request.ReorderPoint,
        request.AllowNegativeStock,
        request.InternalCode,
        request.SupplierCode,
        request.ParentProductId,
        request.VariantName,
        request.AttributesJson,
        request.IsActive),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapGet(
  "/api/catalog/products/{id:guid}",
  async (
    Guid id,
    GetProductHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new GetProductQuery(id), cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status404NotFound);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapPut(
  "/api/catalog/products/{id:guid}/activate",
  async (
    Guid id,
    ActivateProductHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new ActivateProductCommand(id), cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status404NotFound);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapPut(
  "/api/catalog/products/{id:guid}/deactivate",
  async (
    Guid id,
    DeactivateProductHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new DeactivateProductCommand(id), cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status404NotFound);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapGet(
  "/api/catalog/products",
  async (
    [AsParameters] GetProductsEndpointRequest request,
    GetProductsHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetProductsQuery(
        request.Query,
        request.ProductType,
        request.CategoryId,
        request.IsActive,
        request.Page ?? 1,
        request.PageSize ?? 10,
        request.SortBy,
        request.SortDirection),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapPost(
  "/api/catalog/categories",
  async (
    CreateCategoryRequest request,
    CreateCategoryHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new CreateCategoryCommand(request.Name, request.Description),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapPut(
  "/api/catalog/categories/{id:guid}",
  async (
    Guid id,
    UpdateCategoryRequest request,
    UpdateCategoryHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new UpdateCategoryCommand(id, request.Name, request.Description, request.IsActive),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider, StatusCodes.Status404NotFound);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapGet(
  "/api/catalog/categories",
  async (
    GetCategoriesHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(catalogTag);

app.MapPost(
  "/api/inventory/adjustments",
  async (
    CreateInventoryAdjustmentRequest request,
    AdjustInventoryHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new AdjustInventoryCommand(request.ProductId, request.Quantity, request.Reason),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(inventoryTag);

app.MapGet(
  "/api/inventory/stock",
  async (
    [AsParameters] InventoryStockEndpointRequest request,
    GetStockHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetStockQuery(
        request.Search,
        request.LowStockOnly ?? false,
        request.ProductType,
        request.CategoryId,
        request.Page ?? 1,
        request.PageSize ?? 10,
        request.SortBy,
        request.SortDirection),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(inventoryTag);

app.MapGet(
  "/api/inventory/movements",
  async (
    [AsParameters] InventoryMovementsEndpointRequest request,
    GetInventoryMovementsHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetInventoryMovementsQuery(
        request.ProductId,
        request.MovementType,
        request.DateFrom,
        request.DateTo,
        request.Page ?? 1,
        request.PageSize ?? 10,
        request.SortBy,
        request.SortDirection),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(inventoryTag);

app.MapHub<BusinessHub>("/hubs/business")
  .WithTags(realtimeTag);

if (app.Environment.IsDevelopment())
{
  await MigrateDatabaseAsync(app.Services);
  await app.Services.SeedDevelopmentDataAsync();
}

await app.RunAsync();

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
{
  var validationErrors = error.Details?
    .Select(detail => new ValidationError(detail.Code, detail.Message))
    .ToArray();

  return new(error.Code, error.Message, ValidationErrors: validationErrors);
}

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
  protected Program()
  {
  }
}
