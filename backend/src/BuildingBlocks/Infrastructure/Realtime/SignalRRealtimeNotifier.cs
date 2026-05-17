using Microsoft.AspNetCore.SignalR;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Realtime;

public sealed class SignalRRealtimeNotifier(IHubContext<RealtimeHub> hubContext) : IRealtimeNotifier
{
  public Task NotifyBusinessAsync(
    Guid businessId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
    ArgumentNullException.ThrowIfNull(payload);

    return hubContext.Clients
      .Group(RealtimeGroupNames.Business(businessId))
      .SendAsync(eventName, payload, cancellationToken);
  }

  public Task NotifyBranchAsync(
    Guid branchId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
    ArgumentNullException.ThrowIfNull(payload);

    return hubContext.Clients
      .Group(RealtimeGroupNames.Branch(branchId))
      .SendAsync(eventName, payload, cancellationToken);
  }

  public Task NotifyUserAsync(
    Guid userId,
    string eventName,
    object payload,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
    ArgumentNullException.ThrowIfNull(payload);

    return hubContext.Clients
      .Group(RealtimeGroupNames.User(userId))
      .SendAsync(eventName, payload, cancellationToken);
  }
}
