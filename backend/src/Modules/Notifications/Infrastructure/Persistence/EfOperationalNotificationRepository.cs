using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Infrastructure.Persistence;

public sealed class EfOperationalNotificationRepository(AppDbContext db)
  : IOperationalNotificationRepository
{
  public async Task AddAsync(
    OperationalNotification notification,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(notification);
    await db.Set<OperationalNotification>().AddAsync(notification, cancellationToken);
  }

  public Task<OperationalNotification?> GetByIdAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    return db.Set<OperationalNotification>()
      .FirstOrDefaultAsync(
        n => n.Id == id && n.BusinessId == businessId,
        cancellationToken);
  }

  public async Task<(IReadOnlyList<OperationalNotification> Items, int TotalCount)> GetPagedAsync(
    BusinessId businessId,
    Guid? branchId,
    OperationalNotificationStatus? status,
    OperationalNotificationType? type,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
  {
    var query = db.Set<OperationalNotification>()
      .Where(n => n.BusinessId == businessId);

    if (branchId.HasValue)
      query = query.Where(n => n.BranchId == branchId.Value);

    if (status.HasValue)
      query = query.Where(n => n.Status == status.Value);

    if (type.HasValue)
      query = query.Where(n => n.Type == type.Value);

    var total = await query.CountAsync(cancellationToken);

    var items = await query
      .OrderByDescending(n => n.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return (items, total);
  }

  public Task<int> GetUnreadCountAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    return db.Set<OperationalNotification>()
      .CountAsync(
        n => n.BusinessId == businessId && n.Status == OperationalNotificationStatus.Unread,
        cancellationToken);
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    => db.SaveChangesAsync(cancellationToken);
}
