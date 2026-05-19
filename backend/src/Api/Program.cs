using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SaasCommerce.Api;
using SaasCommerce.Api.Endpoints;
using SaasCommerce.Api.Middleware;
using SaasCommerce.Api.Realtime;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;
using SaasCommerce.Modules;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Catalog.Application.Categories;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Requests;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Application.Customers;
using SaasCommerce.Modules.Customers.Contracts.Requests;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Requests;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Application.Suppliers;
using SaasCommerce.Modules.Purchasing.Contracts.Requests;
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
const string invoicesTag = "Invoices";
const string inventoryTag = "Inventory";
const string purchasesTag = "Purchases";
const string realtimeTag = "Realtime";
const string salesTag = "Sales";
const string suppliersTag = "Suppliers";
const string systemTag = "System";
const string tenancyTag = "Tenancy";

ProgramHelpers.ConfigureDevelopmentJwtSecret(builder.Configuration, builder.Environment);

builder.Host.UseSerilog((_, _, loggerConfiguration) =>
  loggerConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

builder.Services.AddModules();
builder.Services.AddBuildingBlocks(
  builder.Configuration,
  massTransit =>
  {
    massTransit.AddConsumer<SaleCompletedRealtimeConsumer>();
    massTransit.AddConsumer<SaleFailedRealtimeConsumer>();
    massTransit.AddConsumer<InventoryAdjustedRealtimeConsumer>();
    massTransit.AddConsumer<InventoryDeductedRealtimeConsumer>();
    massTransit.AddConsumer<LowStockDetectedRealtimeConsumer>();
    massTransit.AddConsumer<PurchaseReceivedRealtimeConsumer>();
    massTransit.AddConsumer<InventoryIncreasedRealtimeConsumer>();
    massTransit.AddConsumer<ProductCostUpdatedRealtimeConsumer>();
    massTransit.AddConsumer<CustomerCreditDebitedRealtimeConsumer>();
    massTransit.AddConsumer<CustomerPaymentRegisteredRealtimeConsumer>();
    massTransit.AddConsumer<InvoiceGeneratedRealtimeConsumer>();
    massTransit.AddConsumer<InvoiceCancelledRealtimeConsumer>();
  });
builder.Services.AddCors(options =>
  options.AddPolicy(
    "Default",
    policy => policy
      .WithOrigins(ProgramHelpers.GetCorsAllowedOrigins(builder.Configuration))
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()));
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
builder.Services.AddAuthorization(options =>
{
  foreach (var permission in ProgramHelpers.GetAllSystemPermissions())
  {
    var captured = permission;
    options.AddPolicy(
      $"Permission:{captured}",
      policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new PermissionRequirement(captured)));
  }
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseStatusCodePages(ProgramHelpers.WriteStatusCodeResponseAsync);

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
  var databaseReady = await ProgramHelpers.CanConnectToDatabaseAsync(dbContext, cancellationToken);
  var rabbitMqReady = await ProgramHelpers.CanConnectToRabbitMqAsync(configuration, cancellationToken);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization()
  .WithTags(identityTag);

app.MapGet(
  "/api/me/permissions",
  (
    GetCurrentUserPermissionsHandler handler,
    ICorrelationIdProvider correlationIdProvider) =>
  {
    var result = handler.Handle(new GetCurrentUserPermissionsQuery());

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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
    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
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

    return ApiHelpers.ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.CustomersCreate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.CustomersView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.CustomersView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.CustomersUpdate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.CustomersUpdate}")
  .WithTags(customersTag);

app.MapPost(
  "/api/customers/{id:guid}/deactivate",
  async (
    Guid id,
    IDeleteCustomerUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new DeleteCustomerCommand(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.CustomersUpdate}")
  .WithTags(customersTag);

app.MapGet(
  "/api/customers/{id:guid}/credit",
  async (
    Guid id,
    IGetCustomerCreditSummaryUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new GetCustomerCreditSummaryQuery(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.AccountsReceivableView}")
  .WithTags(customersTag);

app.MapGet(
  "/api/customers/{id:guid}/credit/movements",
  async (
    Guid id,
    int? page,
    int? pageSize,
    IGetCustomerCreditMovementsUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new GetCustomerCreditMovementsQuery(id, page ?? 1, pageSize ?? 50),
      cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.AccountsReceivableView}")
  .WithTags(customersTag);

app.MapPost(
  "/api/customers/{id:guid}/payments",
  async (
    Guid id,
    RegisterCustomerPaymentRequest request,
    IRegisterCustomerPaymentUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(
      new RegisterCustomerPaymentCommand(id, request.Amount, request.Note),
      cancellationToken);

    return ApiHelpers.ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.AccountsReceivableRegisterPayment}")
  .WithTags(customersTag);

app.MapPost(
  "/api/customers/{id:guid}/credit/block",
  async (
    Guid id,
    IBlockCustomerCreditUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new BlockCustomerCreditCommand(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.AccountsReceivableRegisterPayment}")
  .WithTags(customersTag);

app.MapPost(
  "/api/customers/{id:guid}/credit/unblock",
  async (
    Guid id,
    IUnblockCustomerCreditUseCase useCase,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await useCase.ExecuteAsync(new UnblockCustomerCreditCommand(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.AccountsReceivableRegisterPayment}")
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

    return ApiHelpers.ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.SalesCreate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.SalesView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.SalesView}")
  .WithTags(salesTag);

app.MapGet(
  "/api/sales/{saleId:guid}/invoice",
  async (
    Guid saleId,
    GetInvoiceBySaleHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new GetInvoiceBySaleQuery(saleId), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InvoicesView}")
  .WithTags(invoicesTag);

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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.SalesCancel}")
  .WithTags(salesTag);

app.MapGet(
  "/api/invoices",
  async (
    [AsParameters] InvoiceEndpointRequest request,
    GetInvoicesHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetInvoicesQuery(
        request.Status,
        request.Query,
        request.DateFrom,
        request.DateTo,
        request.Page ?? 1,
        request.PageSize ?? 10),
      cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InvoicesView}")
  .WithTags(invoicesTag);

app.MapGet(
  "/api/invoices/{id:guid}",
  async (
    Guid id,
    GetInvoiceByIdHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new GetInvoiceByIdQuery(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InvoicesView}")
  .WithTags(invoicesTag);

app.MapPost(
  "/api/invoices/{id:guid}/cancel",
  async (
    Guid id,
    CancelInvoiceHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new CancelInvoiceCommand(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InvoicesCancel}")
  .WithTags(invoicesTag);

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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsCreate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsUpdate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsUpdate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsUpdate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsCreate}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsUpdate}")
  .WithTags(catalogTag);

app.MapGet(
  "/api/catalog/categories",
  async (
    GetCategoriesHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.ProductsView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InventoryAdjust}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InventoryView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InventoryView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InventoryView}")
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.InventoryView}")
  .WithTags(inventoryTag);

app.MapPost(
  "/api/suppliers",
  async (
    CreateSupplierRequest request,
    CreateSupplierHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new CreateSupplierCommand(
        request.Name,
        request.Rnc,
        request.Phone,
        request.Email,
        request.Address),
      cancellationToken);

    return ApiHelpers.ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesCreate}")
  .WithTags(suppliersTag);

app.MapPut(
  "/api/suppliers/{id:guid}",
  async (
    Guid id,
    UpdateSupplierRequest request,
    UpdateSupplierHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new UpdateSupplierCommand(
        id,
        request.Name,
        request.Rnc,
        request.Phone,
        request.Email,
        request.Address,
        request.IsActive),
      cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesCreate}")
  .WithTags(suppliersTag);

app.MapGet(
  "/api/suppliers",
  async (
    [AsParameters] SupplierEndpointRequest request,
    GetSuppliersHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetSuppliersQuery(
        request.Query,
        request.IsActive,
        request.Page ?? 1,
        request.PageSize ?? 10,
        request.SortBy,
        request.SortDirection),
      cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesView}")
  .WithTags(suppliersTag);

app.MapPost(
  "/api/purchases",
  async (
    CreatePurchaseRequest request,
    CreatePurchaseHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new CreatePurchaseCommand(
        request.SupplierId,
        request.BranchId,
        request.Items.Select(item => new CreatePurchaseItemCommand(
          item.ProductId,
          item.Quantity,
          item.UnitCost)).ToArray(),
        request.SupplierInvoiceNumber,
        request.PurchaseDate,
        request.Notes,
        request.ReceiveNow),
      cancellationToken);

    return ApiHelpers.ToApiResult(
      result,
      correlationIdProvider,
      successStatusCode: StatusCodes.Status201Created);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesCreate}")
  .WithTags(purchasesTag);

app.MapGet(
  "/api/purchases",
  async (
    [AsParameters] PurchaseEndpointRequest request,
    GetPurchasesHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(
      new GetPurchasesQuery(
        request.SupplierId,
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

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesView}")
  .WithTags(purchasesTag);

app.MapGet(
  "/api/purchases/{id:guid}",
  async (
    Guid id,
    GetPurchaseByIdHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new GetPurchaseByIdQuery(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesView}")
  .WithTags(purchasesTag);

app.MapPost(
  "/api/purchases/{id:guid}/receive",
  async (
    Guid id,
    ReceivePurchaseHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new ReceivePurchaseCommand(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesReceive}")
  .WithTags(purchasesTag);

app.MapPost(
  "/api/purchases/{id:guid}/cancel",
  async (
    Guid id,
    CancelPurchaseHandler handler,
    ICorrelationIdProvider correlationIdProvider,
    CancellationToken cancellationToken) =>
  {
    var result = await handler.Handle(new CancelPurchaseCommand(id), cancellationToken);

    return ApiHelpers.ToApiResult(result, correlationIdProvider);
  })
  .RequireAuthorization($"Permission:{SystemPermissions.PurchasesCancel}")
  .WithTags(purchasesTag);

// ── Dashboard & Reports ────────────────────────────────────────────────────

app.MapReportsEndpoints();

// ── Users & Audit ─────────────────────────────────────────────────────────

app.MapUsersEndpoints();
app.MapAuditEndpoints();

app.MapHub<RealtimeHub>("/hubs/realtime")
  .RequireAuthorization()
  .WithTags(realtimeTag);

if (app.Environment.IsDevelopment())
{
  await ProgramHelpers.MigrateDatabaseAsync(app.Services);
  await app.Services.SeedDevelopmentDataAsync();
}

await app.RunAsync();

public partial class Program
{
  protected Program()
  {
  }
}
