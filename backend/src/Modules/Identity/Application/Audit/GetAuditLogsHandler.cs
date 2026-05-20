using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Audit;

public sealed record GetAuditLogsQuery(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  Guid? UserId,
  string? Action,
  string? EntityName,
  int Page,
  int PageSize);

public sealed class GetAuditLogsHandler(
  IAuditLogReadRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<AuditLogListResponse>> Handle(
    GetAuditLogsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<AuditLogListResponse>(IdentityPermissionsErrors.UserContextRequired);
    }

    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);

    var response = await repository.GetAuditLogsAsync(
      new BusinessId(businessId),
      new AuditLogCriteria(query.DateFrom, query.DateTo, query.UserId, query.Action, query.EntityName, page, pageSize),
      cancellationToken);

    return Result.Success(response);
  }
}
