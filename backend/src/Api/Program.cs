using System.Globalization;
using System.Net.Sockets;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Diagnostics;
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
using SaasCommerce.Modules.Customers.Application.Customers;
using SaasCommerce.Modules.Customers.Contracts.Requests;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Requests;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Requests;
using SaasCommerce.SharedKernel;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
const string accountTag = "Account";
const string authTag = "Auth";
const string catalogTag = "Catalog";
const string customersTag = "Customers";
const string identityTag = "Identity";
const string inventoryTag = "Inventory";
const string realtimeTag = "Realtime";
const string salesTag = "Sales";
const string systemTag = "System";
const string tenancyTag = "Tenancy";
const int generatedJwtSecretBytes = 32;
const int rabbitMqDefaultPort = 5672;
const int readyCheckTimeoutSeconds = 2;

ConfigureDevelopmentJwtSecret(builder.Configuration, builder.Environment);

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
      .AllowAnyMethod()
      .AllowCredentials());
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
app.UseStatusCodePages(WriteStatusCodeResponseAsync);

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

app.MapGet("/health/ready", async (
  AppDbContext dbContext,
  IConfiguration configuration,
  CancellationToken cancellationToken) =>
{
  var databaseReady = await CanConnectToDatabaseAsync(dbContext, cancellationToken);
  var rabbitMqReady = await CanConnectToRabbitMqAsync(configuration, cancellationToken);
  var response = new HealthReadyResponse(
    databaseReady && rabbitMqReady ? "Healthy" : "Unhealthy",
    new HealthDependencyStatus("PostgreSQL", databaseReady ? "Healthy" : "Unhealthy"),
    new HealthDependencyStatus("RabbitMQ", rabbitMqReady ? "Healthy" : "Unhealthy"));

  return databaseReady && rabbitMqReady
    ? Results.Ok(ApiResponse.Success(response))
    : Results.Json(
      ApiResponse.Failure<HealthReadyResponse>(
        new ApiError(ApiErrorCodes.ServiceUnavailable, "One or more dependencies are not ready.")),
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

    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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
    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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
  "/api/customers",
  async (
    CreateCustomerRequest request,
    ICreateCustomerUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new CreateCustomerCommand(request.FullName, request.Phone, request.Email),
      cancellationToken);

    return ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization()
  .WithTags(customersTag);

app.MapGet(
  "/api/customers",
  async (
    [AsParameters] CustomerEndpointRequest request,
    IListCustomersUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new ListCustomersQuery(
        request.Query,
        request.IsActive,
        request.Page ?? 1,
        request.PageSize ?? 10,
        request.SortBy,
        request.SortDirection),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(customersTag);

app.MapGet(
  "/api/customers/{id:guid}",
  async (
    Guid id,
    IGetCustomerByIdUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new GetCustomerByIdQuery(id), cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(customersTag);

app.MapPut(
  "/api/customers/{id:guid}",
  async (
    Guid id,
    UpdateCustomerRequest request,
    IUpdateCustomerUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new UpdateCustomerCommand(id, request.FullName, request.Phone, request.Email, request.IsActive),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(customersTag);

app.MapDelete(
  "/api/customers/{id:guid}",
  async (
    Guid id,
    IDeleteCustomerUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new DeleteCustomerCommand(id), cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(customersTag);

app.MapPost(
  "/api/sales",
  async (
    CreateSaleRequest request,
    ICreateSaleUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new CreateSaleCommand(
        request.BranchId,
        request.CustomerId,
        request.PaymentMethod,
        request.Items
          .Select(item => new CreateSaleItemCommand(item.ProductId, item.Quantity))
          .ToArray()),
      cancellationToken);

    return ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization()
  .WithTags(salesTag);

app.MapGet(
  "/api/sales",
  async (
    [AsParameters] SaleEndpointRequest request,
    IListSalesUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new ListSalesQuery(
        request.BranchId,
        request.Status,
        request.Query,
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
  .WithTags(salesTag);

app.MapGet(
  "/api/sales/{id:guid}",
  async (
    Guid id,
    IGetSaleByIdUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new GetSaleByIdQuery(id), cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(salesTag);

app.MapPost(
  "/api/sales/{id:guid}/cancel",
  async (
    Guid id,
    CancelSaleRequest request,
    ICancelSaleUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new CancelSaleCommand(id, request.Reason), cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(salesTag);

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

    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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

    return ToApiResult(result, correlationIdProvider);
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
      new AdjustInventoryCommand(request.ProductId, request.Quantity, request.Reason, request.BranchId, request.Note),
      cancellationToken);

    return ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(inventoryTag);

app.MapGet(
  "/api/inventory",
  async (
    [AsParameters] InventoryStockEndpointRequest request,
    GetInventoryHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetInventoryQuery(
        request.ProductId,
        request.BranchId,
        request.Search,
        request.LowStockOnly ?? false,
        request.OutOfStockOnly ?? false,
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
  "/api/inventory/products/{productId:guid}",
  async (
    Guid productId,
    GetInventoryProductDetailHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetInventoryProductDetailQuery(productId),
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
        request.ProductId,
        request.BranchId,
        request.Search,
        request.LowStockOnly ?? false,
        request.OutOfStockOnly ?? false,
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

app.MapHub<RealtimeHub>("/hubs/realtime")
  .RequireAuthorization()
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

static async Task<bool> CanConnectToRabbitMqAsync(
  IConfiguration configuration,
  CancellationToken cancellationToken)
{
  if (configuration.GetValue<bool>("RabbitMq:UseInMemory"))
  {
    return true;
  }

  var host = configuration["RabbitMq:Host"] ?? "localhost";
  var port = configuration.GetValue<int?>("RabbitMq:Port") ?? rabbitMqDefaultPort;

  using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
  timeout.CancelAfter(TimeSpan.FromSeconds(readyCheckTimeoutSeconds));

  try
  {
    using var client = new TcpClient();
    await client.ConnectAsync(host, port, timeout.Token);

    return client.Connected;
  }
  catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
  {
    return false;
  }
  catch (SocketException)
  {
    return false;
  }
}

static void ConfigureDevelopmentJwtSecret(
  ConfigurationManager configuration,
  IHostEnvironment environment)
{
  if (!environment.IsDevelopment() ||
      !string.IsNullOrWhiteSpace(configuration["Jwt:Secret"]))
  {
    return;
  }

  configuration["Jwt:Secret"] = Convert.ToBase64String(
    RandomNumberGenerator.GetBytes(generatedJwtSecretBytes));
}

static IResult ToApiResult<T>(
  Result<T> result,
  ICorrelationIdProvider correlationIdProvider,
  int? failureStatusCode = null,
  int? successStatusCode = null)
{
  ArgumentNullException.ThrowIfNull(result);
  ArgumentNullException.ThrowIfNull(correlationIdProvider);

  var apiError = result.IsFailure
    ? ToApiError(result.Error)
    : null;

  return result.IsSuccess
    ? Results.Json(
      ApiResponse.Success(result.Value, correlationIdProvider.CorrelationId),
      statusCode: successStatusCode ?? StatusCodes.Status200OK)
    : Results.Json(
      ApiResponse.Failure<T>(apiError!, correlationIdProvider.CorrelationId),
      statusCode: failureStatusCode ?? ToFailureStatusCode(apiError!.Code));
}

static ApiError ToApiError(DomainError error)
{
  var validationErrors = error.Details?
    .Select(detail => new ValidationError(detail.Code, detail.Message))
    .ToArray();

  return new(ToPublicErrorCode(error.Code), error.Message, ValidationErrors: validationErrors);
}

static string ToPublicErrorCode(string code)
  => code switch
  {
    "validation_error" or
      "catalog.invalid_product" or
      "inventory.invalid_adjustment" or
      "customers.invalid_customer" or
      "sales.invalid_sale" or
      "sales.invalid_state" => ApiErrorCodes.ValidationError,
    "identity.invalid_credentials" or
      "identity.invalid_refresh_token" or
      "identity.not_authenticated" => ApiErrorCodes.Unauthorized,
    "forbidden" => ApiErrorCodes.Forbidden,
    "identity.invalid_current_user" or
      "catalog.user_context_required" or
      "inventory.user_context_required" or
      "customers.user_context_required" or
      "sales.user_context_required" => ApiErrorCodes.TenantContextMissing,
    "identity.user_not_found" or
      "tenancy.business_not_found" or
      "tenancy.branch_not_found" or
      "catalog.category_not_found" or
      "customers.customer_not_found" or
      "sales.sale_not_found" or
      "sales.customer_not_found" => ApiErrorCodes.NotFound,
    "account.duplicate_email" or
      "account.duplicate_identification" or
      "tenancy.duplicate_identification" or
      "catalog.duplicate_category" => ApiErrorCodes.Conflict,
    "catalog.product_not_found" or
      "inventory.product_not_found" or
      "sales.product_not_found" => ApiErrorCodes.ProductNotFound,
    "catalog.duplicate_sku" => ApiErrorCodes.ProductSkuAlreadyExists,
    "catalog.duplicate_barcode" => ApiErrorCodes.ProductBarcodeAlreadyExists,
    "inventory.negative_stock" => ApiErrorCodes.InventoryStockInsufficient,
    "inventory.product_does_not_track_inventory" => ApiErrorCodes.InventoryProductNotTracked,
    _ => code.ToUpperInvariant().Replace('.', '_')
  };

static int ToFailureStatusCode(string publicErrorCode)
  => publicErrorCode switch
  {
    ApiErrorCodes.Unauthorized or
      ApiErrorCodes.TenantContextMissing or
      ApiErrorCodes.AuthUserIdMissing or
      ApiErrorCodes.AuthBusinessIdMissing => StatusCodes.Status401Unauthorized,
    ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
    ApiErrorCodes.NotFound or
      ApiErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
    ApiErrorCodes.Conflict or
      ApiErrorCodes.ProductSkuAlreadyExists or
      ApiErrorCodes.ProductBarcodeAlreadyExists or
      ApiErrorCodes.InventoryStockInsufficient or
      ApiErrorCodes.InventoryProductNotTracked => StatusCodes.Status409Conflict,
    _ => StatusCodes.Status400BadRequest
  };
  
static async Task WriteStatusCodeResponseAsync(StatusCodeContext statusCodeContext)
{
  var httpContext = statusCodeContext.HttpContext;

  if (httpContext.Response.HasStarted)
  {
    return;
  }

  var error = httpContext.Response.StatusCode switch
  {
    StatusCodes.Status401Unauthorized => new ApiError(
      ApiErrorCodes.Unauthorized,
      "Authentication is required."),
    StatusCodes.Status403Forbidden => new ApiError(
      ApiErrorCodes.Forbidden,
      "The current user is not allowed to perform this action."),
    StatusCodes.Status404NotFound => new ApiError(
      ApiErrorCodes.NotFound,
      "The requested resource was not found."),
    _ => null
  };

  if (error is null)
  {
    return;
  }

  httpContext.Response.ContentType = "application/json";

  var correlationId = httpContext.RequestServices
    .GetService<ICorrelationIdProvider>()?
    .CorrelationId ?? httpContext.TraceIdentifier;

  await httpContext.Response.WriteAsJsonAsync(
    ApiResponse.Failure<object?>(error, correlationId));
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
