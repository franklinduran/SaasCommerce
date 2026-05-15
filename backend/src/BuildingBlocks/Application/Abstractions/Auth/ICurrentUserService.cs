namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;

public interface ICurrentUserService
{
  Guid? UserId { get; }

  Guid? BusinessId { get; }

  Guid? BranchId { get; }

  IReadOnlyCollection<string> Roles { get; }

  bool IsAuthenticated { get; }
}
