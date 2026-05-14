namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;

public interface IRealtimeNotifier
{
  Task NotifyBusinessAsync(
    Guid businessId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default);
}
