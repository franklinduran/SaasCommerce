namespace SaasCommerce.BuildingBlocks.Infrastructure.Realtime;

public static class RealtimeGroupNames
{
  public static string Business(Guid businessId) => $"business-{businessId:D}";

  public static string Branch(Guid branchId) => $"branch-{branchId:D}";

  public static string User(Guid userId) => $"user-{userId:D}";
}
