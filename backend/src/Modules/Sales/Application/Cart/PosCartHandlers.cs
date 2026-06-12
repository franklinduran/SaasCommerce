using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Cart;

// ── Commands / Queries ────────────────────────────────────────────────────────

public sealed record GetPosCartQuery;
public sealed record AddCartItemCommand(Guid ProductId, int Quantity);
public sealed record SetCartItemQuantityCommand(Guid ProductId, int Quantity);
public sealed record RemoveCartItemCommand(Guid ProductId);
public sealed record ClearCartCommand;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class PosCartHandler(
  IPosCartRepository cartRepo,
  IProductSalesPolicyReader productPolicies,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  // ── Get ──────────────────────────────────────────────────────────────────────

  public async Task<Result<PosCartResponse>> Handle(
    GetPosCartQuery _,
    CancellationToken ct = default)
  {
    var ctx = ResolveContext();
    if (ctx is null) return Result.Failure<PosCartResponse>(SalesErrors.UserContextRequired);

    var cart = await GetOrCreateAsync(ctx.BusinessId, ctx.UserId, ct);
    return Result.Success(PosCartResponse.From(cart));
  }

  // ── Add / increment ───────────────────────────────────────────────────────

  public async Task<Result<PosCartResponse>> Handle(
    AddCartItemCommand cmd,
    CancellationToken ct = default)
  {
    var ctx = ResolveContext();
    if (ctx is null) return Result.Failure<PosCartResponse>(SalesErrors.UserContextRequired);

    if (cmd.Quantity <= 0)
      return Result.Failure<PosCartResponse>(SalesErrors.InvalidSale);

    var policy = await productPolicies.GetSalesPolicyAsync(ctx.BusinessId, cmd.ProductId, ct);
    if (policy is null || !policy.CanBeSold)
      return Result.Failure<PosCartResponse>(SalesErrors.ProductNotFound);

    var cart = await GetOrCreateAsync(ctx.BusinessId, ctx.UserId, ct);
    cart.AddOrIncrement(cmd.ProductId, policy.Name, policy.Sku, policy.SalePrice, cmd.Quantity, clock.UtcNow);

    await unitOfWork.SaveChangesAsync(ct);
    return Result.Success(PosCartResponse.From(cart));
  }

  // ── Set quantity ──────────────────────────────────────────────────────────

  public async Task<Result<PosCartResponse>> Handle(
    SetCartItemQuantityCommand cmd,
    CancellationToken ct = default)
  {
    var ctx = ResolveContext();
    if (ctx is null) return Result.Failure<PosCartResponse>(SalesErrors.UserContextRequired);

    var cart = await GetOrCreateAsync(ctx.BusinessId, ctx.UserId, ct);
    cart.SetQuantity(cmd.ProductId, cmd.Quantity, clock.UtcNow);

    await unitOfWork.SaveChangesAsync(ct);
    return Result.Success(PosCartResponse.From(cart));
  }

  // ── Remove item ───────────────────────────────────────────────────────────

  public async Task<Result<PosCartResponse>> Handle(
    RemoveCartItemCommand cmd,
    CancellationToken ct = default)
  {
    var ctx = ResolveContext();
    if (ctx is null) return Result.Failure<PosCartResponse>(SalesErrors.UserContextRequired);

    var cart = await GetOrCreateAsync(ctx.BusinessId, ctx.UserId, ct);
    var item = cart.Items.FirstOrDefault(i => i.ProductId == cmd.ProductId);

    if (item is not null)
    {
      cart.RemoveItem(cmd.ProductId, clock.UtcNow);
      await cartRepo.RemoveItemAsync(item, ct);
    }

    await unitOfWork.SaveChangesAsync(ct);
    return Result.Success(PosCartResponse.From(cart));
  }

  // ── Clear ─────────────────────────────────────────────────────────────────

  public async Task<Result> Handle(ClearCartCommand _, CancellationToken ct = default)
  {
    var ctx = ResolveContext();
    if (ctx is null) return Result.Failure(SalesErrors.UserContextRequired);

    var businessId = new BusinessId(ctx.BusinessId);
    var cart = await cartRepo.GetAsync(businessId, ctx.UserId, ct);

    if (cart is not null)
    {
      var items = cart.Items.ToList();
      cart.Clear(clock.UtcNow);
      foreach (var item in items)
        await cartRepo.RemoveItemAsync(item, ct);
      await unitOfWork.SaveChangesAsync(ct);
    }

    return Result.Success();
  }

  // ── Private ───────────────────────────────────────────────────────────────

  private async Task<PosCart> GetOrCreateAsync(Guid businessId, Guid userId, CancellationToken ct)
  {
    var tenantId = new BusinessId(businessId);
    var cart = await cartRepo.GetAsync(tenantId, userId, ct);

    if (cart is not null) return cart;

    cart = new PosCart(Guid.NewGuid(), tenantId, userId, clock.UtcNow);
    await cartRepo.AddAsync(cart, ct);
    return cart;
  }

  private sealed record UserContext(Guid BusinessId, Guid UserId);

  private UserContext? ResolveContext()
  {
    if (currentUser.BusinessId is not Guid biz || currentUser.UserId is not Guid uid)
      return null;
    return new UserContext(biz, uid);
  }
}
