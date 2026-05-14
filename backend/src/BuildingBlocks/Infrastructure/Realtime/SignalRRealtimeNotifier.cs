using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Realtime;

public sealed class SignalRRealtimeNotifier(IHubContext<BusinessHub> hubContext) : IRealtimeNotifier
{
  public Task NotifyBusinessAsync(
    Guid businessId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
    ArgumentNullException.ThrowIfNull(payload);

    var groupName = $"{BusinessHub.BusinessGroupPrefix}:{businessId:D}";

    return hubContext
      .Clients
      .Group(groupName)
      .SendAsync(eventName, payload, cancellationToken);
  }
}
