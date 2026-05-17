using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Infrastructure.Persistence;

public sealed class EfCustomerCreditRepository(AppDbContext dbContext) : ICustomerCreditRepository
{
  public Task<CustomerCreditAccount?> GetAccountAsync(
    BusinessId businessId,
    Guid customerId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CustomerCreditAccount>()
      .SingleOrDefaultAsync(
        account => account.BusinessId == businessId && account.CustomerId == customerId,
        cancellationToken);

  public async Task<IReadOnlyDictionary<Guid, CustomerCreditAccount>> ListAccountsAsync(
    BusinessId businessId,
    IReadOnlyCollection<Guid> customerIds,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(customerIds);

    if (customerIds.Count == 0)
    {
      return new Dictionary<Guid, CustomerCreditAccount>();
    }

    return await dbContext.Set<CustomerCreditAccount>()
      .AsNoTracking()
      .Where(account => account.BusinessId == businessId && customerIds.Contains(account.CustomerId))
      .ToDictionaryAsync(account => account.CustomerId, cancellationToken);
  }

  public Task AddAccountAsync(
    CustomerCreditAccount account,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(account);

    return dbContext.Set<CustomerCreditAccount>().AddAsync(account, cancellationToken).AsTask();
  }

  public Task AddMovementAsync(
    CustomerCreditMovement movement,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(movement);

    return dbContext.Set<CustomerCreditMovement>().AddAsync(movement, cancellationToken).AsTask();
  }

  public Task AddPaymentAsync(
    CustomerPayment payment,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(payment);

    return dbContext.Set<CustomerPayment>().AddAsync(payment, cancellationToken).AsTask();
  }

  public Task<bool> HasDebitForSaleAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CustomerCreditMovement>()
      .AsNoTracking()
      .AnyAsync(
        movement => movement.BusinessId == businessId &&
          movement.SaleId == saleId &&
          movement.Type == CustomerCreditMovementType.Debit,
        cancellationToken);

  public Task<bool> HasPaymentAsync(
    BusinessId businessId,
    Guid paymentId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CustomerPayment>()
      .AsNoTracking()
      .AnyAsync(
        payment => payment.BusinessId == businessId && payment.Id == paymentId,
        cancellationToken);

  public Task<int> CountMovementsAsync(
    BusinessId businessId,
    Guid customerId,
    CancellationToken cancellationToken = default)
    => Movements(businessId, customerId).CountAsync(cancellationToken);

  public async Task<IReadOnlyCollection<CustomerCreditMovement>> ListMovementsAsync(
    BusinessId businessId,
    Guid customerId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    => await Movements(businessId, customerId)
      .OrderByDescending(movement => movement.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToArrayAsync(cancellationToken);

  private IQueryable<CustomerCreditMovement> Movements(BusinessId businessId, Guid customerId)
    => dbContext.Set<CustomerCreditMovement>()
      .AsNoTracking()
      .Where(movement => movement.BusinessId == businessId && movement.CustomerId == customerId);
}
