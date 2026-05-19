using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Domain;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfAuditLogWriter(AppDbContext dbContext, IClock clock) : IAuditLogWriter
{
  public async Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(entry);

    var auditLog = new AuditLog(Guid.NewGuid(), entry, clock.UtcNow);

    await dbContext.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
