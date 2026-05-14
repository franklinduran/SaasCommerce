using Microsoft.AspNetCore.SignalR;

namespace SaasCommerce.Infrastructure.Realtime;

public sealed class BusinessHub : Hub
{
  public const string BusinessGroupPrefix = "business";
}
