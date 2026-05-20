using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Audit;

public sealed record GetAuditLogByIdQuery(Guid AuditLogId);

public sealed class GetAuditLogByIdHandler(
  IAuditLogReadRepository repository,
  ICurrentUserService currentUser)
{
  public async Task<Result<AuditLogDetailResponse>> Handle(
    GetAuditLogByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<AuditLogDetailResponse>(IdentityPermissionsErrors.UserContextRequired);
    }

    var detail = await repository.GetByIdAsync(
      query.AuditLogId,
      new BusinessId(businessId),
      cancellationToken);

    if (detail is null)
    {
      return Result.Failure<AuditLogDetailResponse>(IdentityPermissionsErrors.UserNotFound);
    }

    return Result.Success(detail);
  }
}
