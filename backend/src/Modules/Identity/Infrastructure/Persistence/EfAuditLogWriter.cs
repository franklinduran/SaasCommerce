using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfAuditLogWriter(AppDbContext dbContext, IClock clock) : IAuditLogWriter
{
  public async Task WriteAsync(
    BusinessId businessId,
    Guid? userId,
    string action,
    string entityName,
    Guid? entityId,
    string? description = null,
    string? ipAddress = null,
    CancellationToken cancellationToken = default)
  {
    var entry = new AuditLog(
      Guid.NewGuid(),
      businessId,
      userId,
      action,
      entityName,
      entityId,
      description,
      ipAddress,
      clock.UtcNow);

    await dbContext.Set<AuditLog>().AddAsync(entry, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
