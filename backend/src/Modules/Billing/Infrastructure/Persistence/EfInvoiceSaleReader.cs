using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Infrastructure.Persistence;

public sealed class EfInvoiceSaleReader(AppDbContext dbContext) : IInvoiceSaleReader
{
  public Task<InvoiceSaleSnapshot?> GetAsync(
    Guid businessId,
    Guid saleId,
    CancellationToken cancellationToken = default)
  {
    var tenantId = new BusinessId(businessId);

    return dbContext.Set<Sale>()
      .AsNoTracking()
      .Where(sale => sale.BusinessId == tenantId && sale.Id == saleId)
      .Select(sale => new InvoiceSaleSnapshot(
        sale.Id,
        sale.BusinessId.Value,
        sale.BranchId.Value,
        sale.CustomerId,
        sale.Status.ToString(),
        sale.Total))
      .SingleOrDefaultAsync(cancellationToken);
  }
}
