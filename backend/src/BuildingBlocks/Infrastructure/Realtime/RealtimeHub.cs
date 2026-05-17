using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Realtime;

[Authorize]
public sealed class RealtimeHub(ILogger<RealtimeHub> logger) : Hub
{
  private const string BusinessIdClaim = "business_id";
  private const string BranchIdClaim = "branch_id";
  private const string SubjectClaim = "sub";
  private const string UserIdClaim = ClaimTypes.NameIdentifier;
  private static readonly Action<ILogger, string, bool, bool, Exception?> LogConnectionRejected =
    LoggerMessage.Define<string, bool, bool>(
      LogLevel.Warning,
      new EventId(3000, nameof(LogConnectionRejected)),
      "Realtime connection rejected. ConnectionId={ConnectionId} HasUserId={HasUserId} HasBusinessId={HasBusinessId}");
  private static readonly Action<ILogger, string, Guid, Guid, Guid?, Exception?> LogConnected =
    LoggerMessage.Define<string, Guid, Guid, Guid?>(
      LogLevel.Information,
      new EventId(3001, nameof(LogConnected)),
      "Realtime connected. ConnectionId={ConnectionId} UserId={UserId} BusinessId={BusinessId} BranchId={BranchId}");
  private static readonly Action<ILogger, string, Exception?> LogDisconnected =
    LoggerMessage.Define<string>(
      LogLevel.Information,
      new EventId(3002, nameof(LogDisconnected)),
      "Realtime disconnected. ConnectionId={ConnectionId}");

  public override async Task OnConnectedAsync()
  {
    var userId = GetGuidClaim(UserIdClaim, SubjectClaim);
    var businessId = GetGuidClaim(BusinessIdClaim);
    var branchId = GetGuidClaim(BranchIdClaim);

    if (userId is null || businessId is null)
    {
      LogConnectionRejected(
        logger,
        Context.ConnectionId,
        userId is not null,
        businessId is not null,
        null);
      throw new HubException("Realtime tenant context is missing.");
    }

    await Groups
      .AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.Business(businessId.Value), Context.ConnectionAborted)
      .ConfigureAwait(false);
    await Groups
      .AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.User(userId.Value), Context.ConnectionAborted)
      .ConfigureAwait(false);

    if (branchId is not null)
    {
      await Groups
        .AddToGroupAsync(Context.ConnectionId, RealtimeGroupNames.Branch(branchId.Value), Context.ConnectionAborted)
        .ConfigureAwait(false);
    }

    LogConnected(
      logger,
      Context.ConnectionId,
      userId.Value,
      businessId.Value,
      branchId,
      null);

    await base.OnConnectedAsync().ConfigureAwait(false);
  }

  public override Task OnDisconnectedAsync(Exception? exception)
  {
    LogDisconnected(logger, Context.ConnectionId, exception);

    return base.OnDisconnectedAsync(exception);
  }

  private Guid? GetGuidClaim(params string[] claimTypes)
  {
    foreach (var claimType in claimTypes)
    {
      var value = Context.User?.FindFirstValue(claimType);

      if (Guid.TryParse(value, out var parsedValue))
      {
        return parsedValue;
      }
    }

    return null;
  }
}
