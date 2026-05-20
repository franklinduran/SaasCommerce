using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Audit;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Infrastructure.Persistence;

public sealed class EfAuditLogReadRepository(AppDbContext dbContext) : IAuditLogReadRepository
{
  public async Task<AuditLogListResponse> GetAuditLogsAsync(
    BusinessId businessId,
    AuditLogCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var query = dbContext.Set<AuditLog>()
      .AsNoTracking()
      .Where(log => log.BusinessId == businessId);

    if (criteria.DateFrom.HasValue)
    {
      query = query.Where(log => log.CreatedAt >= criteria.DateFrom.Value);
    }

    if (criteria.DateTo.HasValue)
    {
      query = query.Where(log => log.CreatedAt <= criteria.DateTo.Value);
    }

    if (criteria.UserId.HasValue)
    {
      query = query.Where(log => log.UserId == criteria.UserId.Value);
    }

    if (!string.IsNullOrWhiteSpace(criteria.Action))
    {
      query = query.Where(log => log.Action == criteria.Action);
    }

    if (!string.IsNullOrWhiteSpace(criteria.EntityName))
    {
      query = query.Where(log => log.EntityName == criteria.EntityName);
    }

    var totalItems = await query.CountAsync(cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    // Join with users to get full name
    var userIds = await query
      .Where(log => log.UserId.HasValue)
      .Select(log => log.UserId!.Value)
      .Distinct()
      .ToListAsync(cancellationToken);

    var userNames = await dbContext.Set<User>()
      .AsNoTracking()
      .Where(u => userIds.Contains(u.Id))
      .Select(u => new { u.Id, u.FullName })
      .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

    var rawItems = await query
      .OrderByDescending(log => log.CreatedAt)
      .Skip((criteria.Page - 1) * criteria.PageSize)
      .Take(criteria.PageSize)
      .Select(log => new
      {
        log.Id,
        log.UserId,
        log.Action,
        log.EntityName,
        log.EntityId,
        log.Description,
        log.IpAddress,
        log.CreatedAt
      })
      .ToListAsync(cancellationToken);

    var items = rawItems
      .Select(log => new AuditLogItemResponse(
        log.Id,
        log.UserId,
        log.UserId.HasValue && userNames.TryGetValue(log.UserId.Value, out var name) ? name : null,
        log.Action,
        log.EntityName,
        log.EntityId,
        log.Description,
        log.IpAddress,
        log.CreatedAt))
      .ToArray();

    return new AuditLogListResponse(
      items,
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > criteria.Page);
  }

  public async Task<AuditLogDetailResponse?> GetByIdAsync(
    Guid id,
    BusinessId businessId,
    CancellationToken cancellationToken = default)
  {
    var log = await dbContext.Set<AuditLog>()
      .AsNoTracking()
      .Where(l => l.Id == id && l.BusinessId == businessId)
      .Select(l => new
      {
        l.Id,
        l.UserId,
        l.Action,
        l.EntityName,
        l.EntityId,
        l.Description,
        l.IpAddress,
        l.UserAgent,
        l.CorrelationId,
        l.MetadataJson,
        l.CreatedAt
      })
      .FirstOrDefaultAsync(cancellationToken);

    if (log is null)
    {
      return null;
    }

    string? userFullName = null;
    if (log.UserId.HasValue)
    {
      userFullName = await dbContext.Set<User>()
        .AsNoTracking()
        .Where(u => u.Id == log.UserId.Value)
        .Select(u => u.FullName)
        .FirstOrDefaultAsync(cancellationToken);
    }

    return new AuditLogDetailResponse(
      log.Id,
      log.UserId,
      userFullName,
      log.Action,
      log.EntityName,
      log.EntityId,
      log.Description,
      log.IpAddress,
      log.UserAgent,
      log.CorrelationId,
      log.MetadataJson,
      log.CreatedAt);
  }
}
