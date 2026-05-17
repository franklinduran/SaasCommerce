using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Infrastructure.Realtime;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class RealtimeTests
{
  [Fact]
  public async Task RealtimeHubShouldJoinGroupsFromClaims()
  {
    var userId = Guid.NewGuid();
    var businessId = Guid.NewGuid();
    var branchId = Guid.NewGuid();
    var connectionId = Guid.NewGuid().ToString("D");
    var context = Substitute.For<HubCallerContext>();
    var groups = Substitute.For<IGroupManager>();
    var hub = new RealtimeHub(NullLogger<RealtimeHub>.Instance)
    {
      Context = context,
      Groups = groups
    };

    context.ConnectionId.Returns(connectionId);
    context.ConnectionAborted.Returns(CancellationToken.None);
    context.User.Returns(CreatePrincipal(userId, businessId, branchId));

    await hub.OnConnectedAsync();

    await groups.Received(1).AddToGroupAsync(
      connectionId,
      RealtimeGroupNames.Business(businessId),
      CancellationToken.None);
    await groups.Received(1).AddToGroupAsync(
      connectionId,
      RealtimeGroupNames.Branch(branchId),
      CancellationToken.None);
    await groups.Received(1).AddToGroupAsync(
      connectionId,
      RealtimeGroupNames.User(userId),
      CancellationToken.None);
  }

  [Fact]
  public async Task RealtimeHubShouldRejectMissingTenantClaims()
  {
    var context = Substitute.For<HubCallerContext>();
    var hub = new RealtimeHub(NullLogger<RealtimeHub>.Instance)
    {
      Context = context,
      Groups = Substitute.For<IGroupManager>()
    };

    context.ConnectionId.Returns(Guid.NewGuid().ToString("D"));
    context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity("Test")));

    var act = () => hub.OnConnectedAsync();

    await act.Should().ThrowAsync<HubException>();
  }

  [Fact]
  public async Task RealtimeHubShouldLogDisconnectWithoutFailing()
  {
    var context = Substitute.For<HubCallerContext>();
    var hub = new RealtimeHub(NullLogger<RealtimeHub>.Instance)
    {
      Context = context,
      Groups = Substitute.For<IGroupManager>()
    };

    context.ConnectionId.Returns(Guid.NewGuid().ToString("D"));
    context.User.Returns(CreatePrincipal(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

    var act = () => hub.OnDisconnectedAsync(null);

    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task SignalRRealtimeNotifierShouldSendToBusinessGroup()
  {
    var hubContext = Substitute.For<IHubContext<RealtimeHub>>();
    var clients = Substitute.For<IHubClients>();
    var proxy = Substitute.For<IClientProxy>();
    var businessId = Guid.NewGuid();
    var payload = new { CorrelationId = Guid.NewGuid() };
    hubContext.Clients.Returns(clients);
    clients.Group(RealtimeGroupNames.Business(businessId)).Returns(proxy);
    var notifier = new SignalRRealtimeNotifier(hubContext);

    await notifier.NotifyBusinessAsync(businessId, "realtime.ping", payload);

    await proxy.Received(1).SendCoreAsync(
      "realtime.ping",
      Arg.Is<object?[]>(arguments => arguments.Length == 1 && arguments[0] == payload),
      Arg.Any<CancellationToken>());
    _ = clients.DidNotReceive().All;
  }

  [Fact]
  public async Task SignalRRealtimeNotifierShouldSendToBranchGroup()
  {
    var hubContext = Substitute.For<IHubContext<RealtimeHub>>();
    var clients = Substitute.For<IHubClients>();
    var proxy = Substitute.For<IClientProxy>();
    var branchId = Guid.NewGuid();
    var payload = new { CorrelationId = Guid.NewGuid() };
    hubContext.Clients.Returns(clients);
    clients.Group(RealtimeGroupNames.Branch(branchId)).Returns(proxy);
    var notifier = new SignalRRealtimeNotifier(hubContext);

    await notifier.NotifyBranchAsync(branchId, "inventory.adjusted", payload);

    await proxy.Received(1).SendCoreAsync(
      "inventory.adjusted",
      Arg.Is<object?[]>(arguments => arguments.Length == 1 && arguments[0] == payload),
      Arg.Any<CancellationToken>());
    _ = clients.DidNotReceive().All;
  }

  [Fact]
  public async Task SignalRRealtimeNotifierShouldSendToUserGroup()
  {
    var hubContext = Substitute.For<IHubContext<RealtimeHub>>();
    var clients = Substitute.For<IHubClients>();
    var proxy = Substitute.For<IClientProxy>();
    var userId = Guid.NewGuid();
    var payload = new { CorrelationId = Guid.NewGuid() };
    hubContext.Clients.Returns(clients);
    clients.Group(RealtimeGroupNames.User(userId)).Returns(proxy);
    var notifier = new SignalRRealtimeNotifier(hubContext);

    await notifier.NotifyUserAsync(userId, "sale.statusChanged", payload);

    await proxy.Received(1).SendCoreAsync(
      "sale.statusChanged",
      Arg.Is<object?[]>(arguments => arguments.Length == 1 && arguments[0] == payload),
      Arg.Any<CancellationToken>());
    _ = clients.DidNotReceive().All;
  }

  private static ClaimsPrincipal CreatePrincipal(
    Guid userId,
    Guid businessId,
    Guid branchId)
  {
    var identity = new ClaimsIdentity(
      [
        new Claim(ClaimTypes.NameIdentifier, userId.ToString("D")),
        new Claim("business_id", businessId.ToString("D")),
        new Claim("branch_id", branchId.ToString("D"))
      ],
      "Test");

    return new ClaimsPrincipal(identity);
  }
}
