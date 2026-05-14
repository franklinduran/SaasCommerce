using Microsoft.AspNetCore.SignalR;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Realtime;

public sealed class BusinessHub : Hub
{
  public const string BusinessGroupPrefix = "business";
}
