using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Application.Categories;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Infrastructure.Inventory;
using SaasCommerce.Modules.Catalog.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Application.Settings;
using SaasCommerce.Modules.Identity.Infrastructure.Auth;
using SaasCommerce.Modules.Identity.Infrastructure.Development;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;

namespace SaasCommerce.Modules;

public static class ModulesServiceCollectionExtensions
{
  public static IServiceCollection AddModules(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.AddValidatorsFromAssembly(ModulesAssemblyReference.Assembly);
    services.AddScoped<ICatalogCategoryRepository, EfCatalogCategoryRepository>();
    services.AddScoped<ICatalogProductRepository, EfCatalogProductRepository>();
    services.AddScoped<IProductInventoryPolicyReader, EfProductInventoryPolicyReader>();
    services.AddScoped<IInventoryRepository, EfInventoryRepository>();
    services.AddScoped<IAccountBusinessRepository, EfAccountBusinessRepository>();
    services.AddScoped<IIdentityUserRepository, EfIdentityUserRepository>();
    services.AddScoped<IIdentitySettingsRepository, EfIdentitySettingsRepository>();
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();
    services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
    services.AddScoped<CreateProductHandler>();
    services.AddScoped<UpdateProductHandler>();
    services.AddScoped<ActivateProductHandler>();
    services.AddScoped<DeactivateProductHandler>();
    services.AddScoped<GetProductHandler>();
    services.AddScoped<GetProductsHandler>();
    services.AddScoped<CreateCategoryHandler>();
    services.AddScoped<UpdateCategoryHandler>();
    services.AddScoped<GetCategoriesHandler>();
    services.AddScoped<AdjustInventoryHandler>();
    services.AddScoped<GetStockHandler>();
    services.AddScoped<GetInventoryMovementsHandler>();
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
    services.AddScoped<DevelopmentDataSeeder>();

    return services;
  }
}
