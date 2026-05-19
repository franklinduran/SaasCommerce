using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

public sealed class PurchaseHandlerContext(
    IPurchaseRepository purchases,
    ISupplierRepository suppliers,
    IProductPurchaseReader products,
    ICurrentUserService currentUser,
    IOutboxWriter outbox,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public IPurchaseRepository Purchases { get; } = purchases;
    public ISupplierRepository Suppliers { get; } = suppliers;
    public IProductPurchaseReader Products { get; } = products;
    public ICurrentUserService CurrentUser { get; } = currentUser;
    public IOutboxWriter Outbox { get; } = outbox;
    public IClock Clock { get; } = clock;
    public IUnitOfWork UnitOfWork { get; } = unitOfWork;
}
