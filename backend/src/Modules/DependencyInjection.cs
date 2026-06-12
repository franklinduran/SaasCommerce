using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
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
using SaasCommerce.Modules.Feedback.Application;
using SaasCommerce.Modules.Feedback.Application.Abstractions;
using SaasCommerce.Modules.Feedback.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Application.Export;
using SaasCommerce.Modules.Catalog.Application.Import;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Audit;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Application.Onboarding;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Application.PilotBusiness;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Application.Users;
using SaasCommerce.Modules.Identity.Infrastructure.Onboarding;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Development;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Application.Transfers;
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
using SaasCommerce.Modules.Sales.Application.Cash;
using SaasCommerce.Modules.Sales.Application.CashRegisters;
using SaasCommerce.Modules.Sales.Application.Expenses;
using SaasCommerce.Modules.Sales.Application.DailyClosings;
using SaasCommerce.Modules.Sales.Application.Profitability;
using SaasCommerce.Modules.Sales.Application.Cart;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Infrastructure.Persistence;
using SaasCommerce.Modules.Settings.Application;
using SaasCommerce.Modules.Settings.Infrastructure;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Application.Branches;
using SaasCommerce.Modules.Tenancy.Infrastructure.Persistence;

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
    services.AddScoped<UploadProductImageHandler>();
    services.AddScoped<IPosCartRepository, EfPosCartRepository>();
    services.AddScoped<PosCartHandler>();

    // Inventory
    services.AddScoped<IInventoryRepository, EfInventoryRepository>();
    services.AddScoped<IInventoryReadRepository, EfInventoryReadRepository>();
    services.AddScoped<IInventoryAvailabilityService, EfInventoryAvailabilityService>();
    services.AddScoped(sp => new AdjustInventoryDependencies
    {
      Inventory = sp.GetRequiredService<IInventoryRepository>(),
      ProductPolicies = sp.GetRequiredService<IProductInventoryPolicyReader>(),
      CurrentUser = sp.GetRequiredService<ICurrentUserService>(),
      Outbox = sp.GetRequiredService<IOutboxWriter>(),
      AuditLog = sp.GetRequiredService<IAuditLogWriter>(),
      Clock = sp.GetRequiredService<IClock>(),
      UnitOfWork = sp.GetRequiredService<IUnitOfWork>(),
      SubscriptionAccess = sp.GetRequiredService<ISubscriptionAccessPolicy>()
    });
    services.AddScoped<AdjustInventoryHandler>();
    services.AddScoped<GetInventoryHandler>();
    services.AddScoped<GetInventoryProductDetailHandler>();
    services.AddScoped<GetStockHandler>();
    services.AddScoped<GetInventoryMovementsHandler>();
    services.AddScoped<ExportInventoryCsvHandler>();
    services.AddScoped<IInventoryTransferRepository, EfInventoryTransferRepository>();
    services.AddScoped<IProcessInventoryTransferUseCase, ProcessInventoryTransferUseCase>();
    services.AddScoped<CreateInventoryTransferHandler>();
    services.AddScoped<CancelInventoryTransferHandler>();
    services.AddScoped<GetInventoryTransfersHandler>();
    services.AddScoped<GetInventoryTransferByIdHandler>();

    // Purchasing
    services.AddScoped<ISupplierRepository, EfSupplierRepository>();
    services.AddScoped<IPurchaseRepository, EfPurchaseRepository>();
    services.AddScoped<IPurchaseMovementReader, EfPurchaseMovementReader>();
    services.AddScoped<CreateSupplierHandler>();
    services.AddScoped<UpdateSupplierHandler>();
    services.AddScoped<GetSuppliersHandler>();
    services.AddScoped<PurchaseHandlerContext>();
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

    // Billing - Subscriptions
    services.AddScoped<ISubscriptionPlanRepository, EfSubscriptionPlanRepository>();
    services.AddScoped<IBusinessSubscriptionRepository, EfBusinessSubscriptionRepository>();
    services.AddScoped<ISubscriptionUsageReader, EfSubscriptionUsageReader>();
    services.AddScoped<ISubscriptionAccessPolicy, Billing.Application.Services.SubscriptionAccessPolicy>();
    services.AddScoped<ISubscriptionLimitChecker, Billing.Application.Services.SubscriptionLimitChecker>();

    // Billing - Subscription Handlers
    services.AddScoped<Billing.Application.Subscriptions.Plans.GetSubscriptionPlansQueryHandler>();
    services.AddScoped<Billing.Application.Subscriptions.Plans.GetSubscriptionPlanByIdQueryHandler>();
    services.AddScoped<Billing.Application.Subscriptions.Plans.CreateSubscriptionPlanCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.Plans.UpdateSubscriptionPlanCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.Plans.ActivateSubscriptionPlanCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.Plans.DeactivateSubscriptionPlanCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.StartTrialSubscriptionCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.GetCurrentBusinessSubscriptionQueryHandler>();
    services.AddScoped<Billing.Application.Subscriptions.GetSubscriptionUsageQueryHandler>();
    services.AddScoped<Billing.Application.Subscriptions.ChangeBusinessPlanCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.CancelBusinessSubscriptionCommandHandler>();
    services.AddScoped<Billing.Application.Subscriptions.ReactivateBusinessSubscriptionCommandHandler>();

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
    services.AddScoped(sp => new RegisterCustomerPaymentDependencies
    {
      Customers = sp.GetRequiredService<ICustomerRepository>(),
      Credits = sp.GetRequiredService<ICustomerCreditRepository>(),
      CurrentUser = sp.GetRequiredService<ICurrentUserService>(),
      Outbox = sp.GetRequiredService<IOutboxWriter>(),
      CorrelationIdProvider = sp.GetRequiredService<ICorrelationIdProvider>(),
      Clock = sp.GetRequiredService<IClock>(),
      UnitOfWork = sp.GetRequiredService<IUnitOfWork>(),
      SubscriptionAccess = sp.GetRequiredService<ISubscriptionAccessPolicy>()
    });
    services.AddScoped<IRegisterCustomerPaymentUseCase, RegisterCustomerPaymentUseCase>();
    services.AddScoped<IBlockCustomerCreditUseCase, BlockCustomerCreditUseCase>();
    services.AddScoped<IUnblockCustomerCreditUseCase, UnblockCustomerCreditUseCase>();
    services.AddScoped<IRegisterCreditSaleUseCase, RegisterCreditSaleUseCase>();
    services.AddScoped<ExportCustomersCsvHandler>();
    services.AddScoped<ExportCustomerCreditsCsvHandler>();

    // Sales — Advanced Cash Register (Etapa 34)
    services.AddScoped<ICashRegisterRepository, EfCashRegisterRepository>();
    services.AddScoped<ICashRegisterReadRepository, EfCashRegisterReadRepository>();
    services.AddScoped<ICashRegisterCalculator, EfCashRegisterCalculator>();
    services.AddScoped<OpenCashRegisterHandler>();
    services.AddScoped<RegisterCashRegisterMovementHandler>();
    services.AddScoped<CloseCashRegisterHandler>();
    services.AddScoped<GetActiveCashRegisterHandler>();
    services.AddScoped<GetCashRegisterDetailHandler>();
    services.AddScoped<GetCashRegistersHandler>();
    services.AddScoped<GetDailyCashRegisterSummaryHandler>();

    // Sales — Cash Register
    services.AddScoped<ICashSessionRepository, EfCashSessionRepository>();
    services.AddScoped<OpenCashSessionHandler>();
    services.AddScoped<CloseCashSessionHandler>();
    services.AddScoped<RegisterCashMovementHandler>();
    services.AddScoped<GetCurrentCashSessionHandler>();
    services.AddScoped<GetCashSessionDetailHandler>();
    services.AddScoped<GetCashSessionsHandler>();
    services.AddScoped<ExportCashSessionsCsvHandler>();

    // Sales — Profitability (Rentabilidad)
    services.AddScoped<IProfitabilityReadRepository, EfProfitabilityReadRepository>();
    services.AddScoped<GetProfitabilitySummaryHandler>();
    services.AddScoped<GetProductProfitabilityHandler>();
    services.AddScoped<GetBranchProfitabilityHandler>();
    services.AddScoped<GetProfitabilityAlertsHandler>();

    // Sales — Daily Closing (Cierre Operativo Diario)
    services.AddScoped<IDailyClosingRepository, EfDailyClosingRepository>();
    services.AddScoped<IDailyClosingReadRepository, EfDailyClosingReadRepository>();
    services.AddScoped<IDailyClosingDataGatherer, EfDailyClosingDataGatherer>();
    services.AddScoped<PreviewDailyClosingHandler>();
    services.AddScoped<CreateDailyClosingHandler>();
    services.AddScoped<CloseDailyClosingHandler>();
    services.AddScoped<GetDailyClosingsHandler>();
    services.AddScoped<GetDailyClosingDetailHandler>();
    services.AddScoped<ExportDailyClosingsCsvHandler>();

    // Sales — Operating Expenses (Gastos Operativos)
    services.AddScoped<IExpenseCategoryRepository, EfExpenseCategoryRepository>();
    services.AddScoped<IOperatingExpenseRepository, EfOperatingExpenseRepository>();
    services.AddScoped<CreateExpenseCategoryHandler>();
    services.AddScoped<GetExpenseCategoriesHandler>();
    services.AddScoped<CreateOperatingExpenseHandler>();
    services.AddScoped<PayOperatingExpenseHandler>();
    services.AddScoped<CancelOperatingExpenseHandler>();
    services.AddScoped<GetOperatingExpensesHandler>();
    services.AddScoped<GetOperatingExpenseDetailHandler>();
    services.AddScoped<GetExpenseSummaryHandler>();

    // Sales
    services.AddScoped<ISaleRepository, EfSaleRepository>();
    services.AddScoped<ISaleReadRepository, EfSaleReadRepository>();
    services.AddScoped<ISaleReturnRepository, EfSaleReturnRepository>();
    services.AddScoped<ISaleReturnReadRepository, EfSaleReturnReadRepository>();
    services.AddScoped<SaleHandlerContext>();
    services.AddScoped(sp => new RequestSaleReturnDependencies
    {
      Sales = sp.GetRequiredService<ISaleRepository>(),
      Returns = sp.GetRequiredService<ISaleReturnRepository>(),
      ReturnReads = sp.GetRequiredService<ISaleReturnReadRepository>(),
      CurrentUser = sp.GetRequiredService<ICurrentUserService>(),
      Outbox = sp.GetRequiredService<IOutboxWriter>(),
      CorrelationIdProvider = sp.GetRequiredService<ICorrelationIdProvider>(),
      Clock = sp.GetRequiredService<IClock>(),
      UnitOfWork = sp.GetRequiredService<IUnitOfWork>()
    });
    services.AddScoped<ICreateSaleUseCase, CreateSaleUseCase>();
    services.AddScoped<ISaleEventWriter, SaleEventWriter>();
    services.AddScoped<IGetSaleByIdUseCase, GetSaleByIdUseCase>();
    services.AddScoped<IListSalesUseCase, ListSalesUseCase>();
    services.AddScoped<ICancelSaleUseCase, CancelSaleUseCase>();
    services.AddScoped<IRequestSaleReturnUseCase, RequestSaleReturnHandler>();
    services.AddScoped<IApproveSaleReturnUseCase, ApproveSaleReturnUseCase>();
    services.AddScoped<IRestoreInventoryFromSaleReturnUseCase, RestoreInventoryFromSaleReturnUseCase>();
    services.AddScoped<IGenerateCreditNoteForReturnUseCase, GenerateCreditNoteForReturnUseCase>();
    services.AddScoped<GetSaleReturnByIdHandler>();
    services.AddScoped<ListSaleReturnsHandler>();
    services.AddScoped<IValidateSaleStockUseCase, ValidateSaleStockUseCase>();
    services.AddScoped<IDeductSaleInventoryUseCase, DeductSaleInventoryUseCase>();
    services.AddScoped<IRegisterSalePaymentUseCase, RegisterSalePaymentUseCase>();
    services.AddScoped<IGenerateSaleInvoiceUseCase, GenerateSaleInvoiceUseCase>();
    services.AddScoped<ICompleteSaleUseCase, CompleteSaleUseCase>();
    services.AddScoped<IFailSaleUseCase, FailSaleUseCase>();
    services.AddScoped<ExportSalesCsvHandler>();

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
    services.AddScoped<GetAuditLogByIdHandler>();

    // Identity — user management
    services.AddScoped<IUserManagementRepository, EfUserManagementRepository>();
    services.AddScoped<GetUsersHandler>();
    services.AddScoped<GetUserByIdHandler>();
    services.AddScoped(sp => new CreateUserDependencies
    {
      Repository = sp.GetRequiredService<IUserManagementRepository>(),
      CurrentUser = sp.GetRequiredService<ICurrentUserService>(),
      PasswordHasher = sp.GetRequiredService<IPasswordHasher>(),
      Clock = sp.GetRequiredService<IClock>(),
      UnitOfWork = sp.GetRequiredService<IUnitOfWork>(),
      AuditLogWriter = sp.GetRequiredService<IAuditLogWriter>(),
      EventBus = sp.GetRequiredService<IEventBus>(),
      LimitChecker = sp.GetRequiredService<ISubscriptionLimitChecker>()
    });
    services.AddScoped<CreateUserHandler>();
    services.AddScoped<UpdateUserHandler>();
    services.AddScoped<ActivateUserHandler>();
    services.AddScoped<ResetUserPasswordHandler>();
    services.AddScoped<UpdateUserRoleHandler>();
    services.AddScoped<DisableUserHandler>();

    // Identity — pilot business / metrics
    services.AddScoped(sp => new CreatePilotBusinessDependencies
    {
      Businesses = sp.GetRequiredService<IAccountBusinessRepository>(),
      Users = sp.GetRequiredService<IIdentityUserRepository>(),
      PasswordHasher = sp.GetRequiredService<IPasswordHasher>(),
      SubscriptionPlans = sp.GetRequiredService<ISubscriptionPlanRepository>(),
      Subscriptions = sp.GetRequiredService<IBusinessSubscriptionRepository>(),
      Clock = sp.GetRequiredService<IClock>(),
      UnitOfWork = sp.GetRequiredService<IUnitOfWork>(),
      AuditLogWriter = sp.GetRequiredService<IAuditLogWriter>(),
      CurrentUser = sp.GetRequiredService<ICurrentUserService>()
    });
    services.AddScoped<CreatePilotBusinessHandler>();
    services.AddScoped<IPilotMetricsRepository, EfPilotMetricsRepository>();
    services.AddScoped<GetPilotMetricsHandler>();

    // Identity — onboarding
    services.AddScoped<IOnboardingStatusReader, EfOnboardingStatusReader>();
    services.AddScoped<GetOnboardingStatusHandler>();
    services.AddScoped<CompleteOnboardingStepHandler>();

    // Catalog — import / export
    services.AddScoped<ImportProductsHandler>();
    services.AddScoped<ExportProductsCsvHandler>();

    // Identity — application handlers
    services.AddScoped(sp => new RegisterBusinessDependencies
    {
      Businesses = sp.GetRequiredService<IAccountBusinessRepository>(),
      Users = sp.GetRequiredService<IIdentityUserRepository>(),
      PasswordHasher = sp.GetRequiredService<IPasswordHasher>(),
      JwtTokenService = sp.GetRequiredService<IJwtTokenService>(),
      RefreshTokenGenerator = sp.GetRequiredService<IRefreshTokenGenerator>(),
      SubscriptionPlans = sp.GetRequiredService<ISubscriptionPlanRepository>(),
      Subscriptions = sp.GetRequiredService<IBusinessSubscriptionRepository>(),
      Clock = sp.GetRequiredService<IClock>(),
      UnitOfWork = sp.GetRequiredService<IUnitOfWork>()
    });
    services.AddScoped<RegisterBusinessHandler>();
    services.AddScoped<LoginHandler>();
    services.AddScoped<RefreshTokenHandler>();
    services.AddScoped<LogoutHandler>();
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

    // Beta feedback
    services.AddScoped<IBetaFeedbackRepository, EfBetaFeedbackRepository>();
    services.AddScoped<CreateBetaFeedbackHandler>();
    services.AddScoped<GetBetaFeedbackHandler>();
    services.AddScoped<UpdateBetaFeedbackStatusHandler>();

    // Settings
    services.AddScoped<IBusinessSettingsRepository, EfBusinessSettingsRepository>();
    services.AddScoped<ISalesSettingsRepository, EfSalesSettingsRepository>();
    services.AddScoped<IInventorySettingsRepository, EfInventorySettingsRepository>();
    services.AddScoped<IBillingSettingsRepository, EfBillingSettingsRepository>();
    services.AddScoped<GetBusinessSettingsHandler>();
    services.AddScoped<UpdateBusinessSettingsHandler>();
    services.AddScoped<GetSalesSettingsHandler>();
    services.AddScoped<UpdateSalesSettingsHandler>();
    services.AddScoped<GetInventorySettingsHandler>();
    services.AddScoped<UpdateInventorySettingsHandler>();
    services.AddScoped<GetBillingSettingsHandler>();
    services.AddScoped<UpdateBillingSettingsHandler>();

    // Branches
    services.AddScoped<IBranchRepository, EfBranchRepository>();
    services.AddScoped<CreateBranchHandler>();
    services.AddScoped<UpdateBranchHandler>();
    services.AddScoped<ActivateBranchHandler>();
    services.AddScoped<DeactivateBranchHandler>();
    services.AddScoped<GetBranchesHandler>();
    services.AddScoped<GetBranchByIdHandler>();

    // Reporting
    services.AddScoped<IReportsReadRepository, EfReportsReadRepository>();
    services.AddScoped<IReportExportService, CsvReportExportService>();
    services.AddScoped<GetDashboardSummaryHandler>();
    services.AddScoped<GetSalesReportHandler>();
    services.AddScoped<GetInvoiceReportHandler>();
    services.AddScoped<GetAccountsReceivableReportHandler>();
    services.AddScoped<GetLowStockReportHandler>();
    services.AddScoped<GetPurchaseReportHandler>();

    // Notifications
    services.AddScoped<IOperationalNotificationRepository, EfOperationalNotificationRepository>();
    services.AddScoped<CreateNotificationHandler>();
    services.AddScoped<GetNotificationsHandler>();
    services.AddScoped<GetUnreadCountHandler>();
    services.AddScoped<MarkNotificationReadHandler>();
    services.AddScoped<MarkAllNotificationsReadHandler>();

    return services;
  }
}
