using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Infrastructure.Persistence;

public sealed class EfSaleReturnRepository(AppDbContext dbContext) : ISaleReturnRepository
{
  public Task<SaleReturn?> GetAsync(
    BusinessId businessId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default)
    => Returns(businessId)
      .SingleOrDefaultAsync(saleReturn => saleReturn.Id == saleReturnId, cancellationToken);

  public Task<SaleReturn?> GetBySaleAsync(
    BusinessId businessId,
    Guid saleId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default)
    => Returns(businessId)
      .SingleOrDefaultAsync(
        saleReturn => saleReturn.Id == saleReturnId && saleReturn.SaleId == saleId,
        cancellationToken);

  public Task<CreditNote?> GetCreditNoteByReturnAsync(
    BusinessId businessId,
    Guid saleReturnId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<CreditNote>()
      .Include(note => note.Items)
      .SingleOrDefaultAsync(
        note => note.BusinessId == businessId && note.SaleReturnId == saleReturnId,
        cancellationToken);

  public async Task<IReadOnlyDictionary<Guid, decimal>> GetReturnedQuantityBySaleItemAsync(
    BusinessId businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
  {
    var rows = await (
      from saleReturn in dbContext.Set<SaleReturn>().AsNoTracking()
      from item in saleReturn.Items
      where saleReturn.BusinessId == businessId &&
        saleReturn.SaleId == saleId &&
        saleReturn.Status != SaleReturnStatus.Failed
      group item by item.SaleItemId into grouped
      select new { SaleItemId = grouped.Key, Quantity = grouped.Sum(item => item.Quantity) })
      .ToArrayAsync(cancellationToken);

    return rows.ToDictionary(row => row.SaleItemId, row => row.Quantity);
  }

  public Task AddAsync(SaleReturn saleReturn, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(saleReturn);

    return dbContext.Set<SaleReturn>().AddAsync(saleReturn, cancellationToken).AsTask();
  }

  public Task AddCreditNoteAsync(CreditNote creditNote, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(creditNote);

    return dbContext.Set<CreditNote>().AddAsync(creditNote, cancellationToken).AsTask();
  }

  private IQueryable<SaleReturn> Returns(BusinessId businessId)
    => dbContext.Set<SaleReturn>()
      .Include(saleReturn => saleReturn.Items)
      .Where(saleReturn => saleReturn.BusinessId == businessId);
}
