using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Abstractions;

public interface ICustomerCreditRepository
{
  Task<CustomerCreditAccount?> GetAccountAsync(
    BusinessId businessId,
    Guid customerId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyDictionary<Guid, CustomerCreditAccount>> ListAccountsAsync(
    BusinessId businessId,
    IReadOnlyCollection<Guid> customerIds,
    CancellationToken cancellationToken = default);

  Task AddAccountAsync(
    CustomerCreditAccount account,
    CancellationToken cancellationToken = default);

  Task AddMovementAsync(
    CustomerCreditMovement movement,
    CancellationToken cancellationToken = default);

  Task AddPaymentAsync(
    CustomerPayment payment,
    CancellationToken cancellationToken = default);

  Task<bool> HasDebitForSaleAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task<bool> HasPaymentAsync(
    BusinessId businessId,
    Guid paymentId,
    CancellationToken cancellationToken = default);

  Task<int> CountMovementsAsync(
    BusinessId businessId,
    Guid customerId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<CustomerCreditMovement>> ListMovementsAsync(
    BusinessId businessId,
    Guid customerId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<CustomerCreditAccount>> ExportAllAccountsAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}
