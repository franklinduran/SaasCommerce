using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Abstractions;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class SaleHandlerContext(
    ISaleRepository sales,
    ICustomerRepository customers,
    IProductSalesPolicyReader productPolicies,
    ICurrentUserService currentUser,
    ISaleEventWriter saleEvents,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public ISaleRepository Sales { get; } = sales;
    public ICustomerRepository Customers { get; } = customers;
    public IProductSalesPolicyReader ProductPolicies { get; } = productPolicies;
    public ICurrentUserService CurrentUser { get; } = currentUser;
    public ISaleEventWriter SaleEvents { get; } = saleEvents;
    public IClock Clock { get; } = clock;
    public IUnitOfWork UnitOfWork { get; } = unitOfWork;
}
