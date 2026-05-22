using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Inventory.Application.Transfers;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Infrastructure.Persistence;

public sealed class EfInventoryTransferRepository(AppDbContext dbContext) : IInventoryTransferRepository
{
  public Task<InventoryTransfer?> GetByIdAsync(
    BusinessId businessId,
    Guid transferId,
    CancellationToken cancellationToken = default)
    => dbContext.Set<InventoryTransfer>()
      .Include(transfer => transfer.Items)
      .SingleOrDefaultAsync(
        transfer => transfer.BusinessId == businessId && transfer.Id == transferId,
        cancellationToken);

  public async Task<(IReadOnlyCollection<InventoryTransfer> Items, int Total)> ListAsync(
    BusinessId businessId,
    Guid? sourceBranchId,
    Guid? targetBranchId,
    InventoryTransferStatus? status,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<InventoryTransfer>()
      .Include(transfer => transfer.Items)
      .AsNoTracking()
      .Where(transfer => transfer.BusinessId == businessId);

    if (sourceBranchId.HasValue)
    {
      var sourceBranch = new BranchId(sourceBranchId.Value);
      query = query.Where(transfer => transfer.SourceBranchId == sourceBranch);
    }

    if (targetBranchId.HasValue)
    {
      var targetBranch = new BranchId(targetBranchId.Value);
      query = query.Where(transfer => transfer.TargetBranchId == targetBranch);
    }

    if (status.HasValue)
    {
      query = query.Where(transfer => transfer.Status == status.Value);
    }

    var total = await query.CountAsync(cancellationToken);

    var items = await query
      .OrderByDescending(transfer => transfer.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToArrayAsync(cancellationToken);

    return (items, total);
  }

  public Task AddAsync(InventoryTransfer transfer, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(transfer);

    return dbContext.Set<InventoryTransfer>().AddAsync(transfer, cancellationToken).AsTask();
  }
}
