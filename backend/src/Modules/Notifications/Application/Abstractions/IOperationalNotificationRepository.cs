using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Application.Abstractions;

public interface IOperationalNotificationRepository
{
  Task AddAsync(OperationalNotification notification, CancellationToken cancellationToken = default);

  Task<OperationalNotification?> GetByIdAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default);

  Task<(IReadOnlyList<OperationalNotification> Items, int TotalCount)> GetPagedAsync(
    BusinessId businessId,
    Guid? branchId,
    OperationalNotificationStatus? status,
    OperationalNotificationType? type,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

  Task<int> GetUnreadCountAsync(BusinessId businessId, CancellationToken cancellationToken = default);

  Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
