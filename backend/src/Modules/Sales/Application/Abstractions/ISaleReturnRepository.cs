using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Abstractions;

public interface ISaleReturnRepository
{
  Task<SaleReturn?> GetAsync(
    BusinessId businessId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default);

  Task<SaleReturn?> GetBySaleAsync(
    BusinessId businessId,
    Guid saleId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default);

  Task<CreditNote?> GetCreditNoteByReturnAsync(
    BusinessId businessId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyDictionary<Guid, decimal>> GetReturnedQuantityBySaleItemAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default);

  Task AddAsync(SaleReturn saleReturn, CancellationToken cancellationToken = default);

  Task AddCreditNoteAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
}
