namespace SaasCommerce.Domain.Common;

public interface ITenantScopedEntity
{
  Guid BusinessId { get; }
}
