namespace SaasCommerce.SharedKernel.Tenancy;

public interface ITenantScopedEntity
{
  Guid BusinessId { get; }
}
