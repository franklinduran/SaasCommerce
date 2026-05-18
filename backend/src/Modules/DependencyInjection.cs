using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Infrastructure.Auth;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Billing.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Application.Categories;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Catalog.Infrastructure.Inventory;
using SaasCommerce.Modules.Catalog.Infrastructure.Persistence;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Application.Customers;
using SaasCommerce.Modules.Customers.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Audit;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Application.Users;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Development;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Infrastructure.Availability;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Application.Suppliers;
using SaasCommerce.Modules.Purchasing.Infrastructure.Persistence;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Application.Dashboard;
using SaasCommerce.Modules.Reporting.Application.Reports;
using SaasCommerce.Modules.Reporting.Infrastructure.Export;
using SaasCommerce.Modules.Reporting.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Infrastructure.Persistence;

namespace SaasCommerce.Modules;

public static class ModulesServiceCollectionExtensions
{
  public static IServiceCollection AddModules(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddValidatorsFromAssembly(ModulesAssemblyReference.Assembly);

    // Catalog
    services.AddScoped<ICatalogCategoryRepository, EfCatalogCategoryRepository>();
    services.AddScoped<ICatalogProductRepository, EfCatalogProductRepository>();
    services.AddScoped<IProductInventoryPolicyReader, EfProductInventoryPolicyReader>();
    services.AddScoped<IInventoryProductLookupReader, EfProductInventoryPolicyReader>();
    services.AddScoped<IProductSalesPolicyReader, EfProductInventoryPolicyReader>();
    services.AddScoped<IProductPurchaseReader, EfProductPurchaseReader>();
    services.AddScoped<CreateProductHandler>();
    services.AddScoped<UpdateProductHandler>();
    services.AddScoped<ActivateProductHandler>();
    services.AddScoped<DeactivateProductHandler>();
    services.AddScoped<GetProductHandler>();
    services.AddScoped<GetProductsHandler>();
    services.AddScoped<CreateCategoryHandler>();
    services.AddScoped<UpdateCategoryHandler>();
    services.AddScoped<GetCategoriesHandler>();

    // Inventory
    services.AddScoped<IInventoryRepository, EfInventoryRepository>();
    services.AddScoped<IInventoryReadRepository, EfInventoryReadRepository>();
    services.AddScoped<IInventoryAvailabilityService, EfInventoryAvailabilityService>();
    services.AddScoped<AdjustInventoryHandler>();
    services.AddScoped<GetInventoryHandler>();
    services.AddScoped<GetInventoryProductDetailHandler>();
    services.AddScoped<GetStockHandler>();
    services.AddScoped<GetInventoryMovementsHandler>();

    // Purchasing
    services.AddScoped<ISupplierRepository, EfSupplierRepository>();
    services.AddScoped<IPurchaseRepository, EfPurchaseRepository>();
    services.AddScoped<IPurchaseMovementReader, EfPurchaseMovementReader>();
    services.AddScoped<CreateSupplierHandler>();
    services.AddScoped<UpdateSupplierHandler>();
    services.AddScoped<GetSuppliersHandler>();
    services.AddScoped<PurchaseReceiptProcessor>();
    services.AddScoped<CreatePurchaseHandler>();
    services.AddScoped<ReceivePurchaseHandler>();
    services.AddScoped<CancelPurchaseHandler>();
    services.AddScoped<GetPurchasesHandler>();
    services.AddScoped<GetPurchaseByIdHandler>();
    services.AddScoped<IProcessPurchaseReceivedEventUseCase, ProcessPurchaseReceivedEventUseCase>();

    // Billing
    services.AddScoped<IInvoiceRepository, EfInvoiceRepository>();
    services.AddScoped<IInvoiceSaleReader, EfInvoiceSaleReader>();
    services.AddScoped<IGenerateInvoiceUseCase, GenerateInvoiceHandler>();
    services.AddScoped<GetInvoiceBySaleHandler>();
    services.AddScoped<GetInvoiceByIdHandler>();
    services.AddScoped<GetInvoicesHandler>();
    services.AddScoped<CancelInvoiceHandler>();

    // Customers
    services.AddScoped<ICustomerRepository, EfCustomerRepository>();
    services.AddScoped<ICustomerCreditRepository, EfCustomerCreditRepository>();
    services.AddScoped<ICreateCustomerUseCase, CreateCustomerUseCase>();
    services.AddScoped<IGetCustomerByIdUseCase, GetCustomerByIdUseCase>();
    services.AddScoped<IListCustomersUseCase, ListCustomersUseCase>();
    services.AddScoped<IUpdateCustomerUseCase, UpdateCustomerUseCase>();
    services.AddScoped<IDeleteCustomerUseCase, DeleteCustomerUseCase>();
    services.AddScoped<IGetCustomerCreditSummaryUseCase, GetCustomerCreditSummaryUseCase>();
    services.AddScoped<IGetCustomerCreditMovementsUseCase, GetCustomerCreditMovementsUseCase>();
    services.AddScoped<IRegisterCustomerPaymentUseCase, RegisterCustomerPaymentUseCase>();
    services.AddScoped<IBlockCustomerCreditUseCase, BlockCustomerCreditUseCase>();
    services.AddScoped<IUnblockCustomerCreditUseCase, UnblockCustomerCreditUseCase>();
    services.AddScoped<IRegisterCreditSaleUseCase, RegisterCreditSaleUseCase>();

    // Sales
    services.AddScoped<ISaleRepository, EfSaleRepository>();
    services.AddScoped<ISaleReadRepository, EfSaleReadRepository>();
    services.AddScoped<ICreateSaleUseCase, CreateSaleUseCase>();
    services.AddScoped<ISaleEventWriter, SaleEventWriter>();
    services.AddScoped<IGetSaleByIdUseCase, GetSaleByIdUseCase>();
    services.AddScoped<IListSalesUseCase, ListSalesUseCase>();
    services.AddScoped<ICancelSaleUseCase, CancelSaleUseCase>();
    services.AddScoped<IValidateSaleStockUseCase, ValidateSaleStockUseCase>();
    services.AddScoped<IDeductSaleInventoryUseCase, DeductSaleInventoryUseCase>();
    services.AddScoped<IRegisterSalePaymentUseCase, RegisterSalePaymentUseCase>();
    services.AddScoped<IGenerateSaleInvoiceUseCase, GenerateSaleInvoiceUseCase>();
    services.AddScoped<ICompleteSaleUseCase, CompleteSaleUseCase>();
    services.AddScoped<IFailSaleUseCase, FailSaleUseCase>();

    // Identity — auth infrastructure
    services.AddScoped<IAccountBusinessRepository, EfAccountBusinessRepository>();
    services.AddScoped<IIdentityUserRepository, EfIdentityUserRepository>();
    services.AddScoped<IIdentitySettingsRepository, EfIdentitySettingsRepository>();
    services.AddScoped<IJwtTokenService, Identity.Infrastructure.Auth.JwtTokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

    // Identity — security: permissions & authorization
    services.AddScoped<IPermissionService, PermissionService>();
    services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    // Identity — audit log
    services.AddScoped<IAuditLogWriter, EfAuditLogWriter>();
    services.AddScoped<IAuditLogReadRepository, EfAuditLogReadRepository>();
    services.AddScoped<GetAuditLogsHandler>();

    // Identity — user management
    services.AddScoped<IUserManagementRepository, EfUserManagementRepository>();
    services.AddScoped<GetUsersHandler>();
    services.AddScoped<UpdateUserRoleHandler>();
    services.AddScoped<DisableUserHandler>();

    // Identity — application handlers
    services.AddScoped<RegisterBusinessHandler>();
    services.AddScoped<LoginHandler>();
    services.AddScoped<RefreshTokenHandler>();
    services.AddScoped<GetCurrentUserHandler>();
    services.AddScoped<GetMeHandler>();
    services.AddScoped<UpdateMyProfileHandler>();
    services.AddScoped<ChangeMyPasswordHandler>();
    services.AddScoped<GetCurrentBusinessHandler>();
    services.AddScoped<UpdateCurrentBusinessHandler>();
    services.AddScoped<GetCurrentBranchHandler>();
    services.AddScoped<UpdateCurrentBranchHandler>();
    services.AddScoped<GetCurrentUserPermissionsHandler>();
    services.AddScoped<DevelopmentDataSeeder>();

    // Reporting
    services.AddScoped<IReportsReadRepository, EfReportsReadRepository>();
    services.AddScoped<IReportExportService, CsvReportExportService>();
    services.AddScoped<GetDashboardSummaryHandler>();
    services.AddScoped<GetSalesReportHandler>();
    services.AddScoped<GetInvoiceReportHandler>();
    services.AddScoped<GetAccountsReceivableReportHandler>();
    services.AddScoped<GetLowStockReportHandler>();
    services.AddScoped<GetPurchaseReportHandler>();

    return services;
  }
}
