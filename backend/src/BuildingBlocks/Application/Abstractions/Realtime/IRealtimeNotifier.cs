namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;

public interface IRealtimeNotifier
{
  Task NotifyBusinessAsync(
    Guid businessId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default);

  Task NotifyBranchAsync(
    Guid branchId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default);

  Task NotifyUserAsync(
    Guid userId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default);
}
