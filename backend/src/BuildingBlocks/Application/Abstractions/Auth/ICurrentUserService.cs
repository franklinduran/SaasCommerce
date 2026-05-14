namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;

public interface ICurrentUserService
{
  Guid? UserId { get; }

  Guid? BusinessId { get; }

  bool IsAuthenticated { get; }
}
