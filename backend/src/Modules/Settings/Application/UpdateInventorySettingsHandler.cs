using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Settings.Contracts.Responses;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Settings.Application;

public sealed record UpdateInventorySettingsCommand(
  bool EnableLowStockAlerts,
  decimal DefaultLowStockThreshold,
  bool RequireReasonForInventoryAdjustment,
  bool AllowInventoryTransferBetweenBranches);

public sealed class UpdateInventorySettingsHandler(
  IInventorySettingsRepository repository,
  ICurrentUserService currentUser,
  IAuditLogWriter auditLog,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<InventorySettingsResponse>> Handle(
    UpdateInventorySettingsCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<InventorySettingsResponse>> HandleCoreAsync(
    UpdateInventorySettingsCommand command,
    CancellationToken cancellationToken)
  {
    if (!currentUser.IsAuthenticated ||
        currentUser.BusinessId is not { } businessIdValue ||
        currentUser.UserId is not { } userId)
    {
      return Result.Failure<InventorySettingsResponse>(SettingsErrors.UserContextRequired);
    }

    if (!currentUser.Roles.Contains("Admin", StringComparer.Ordinal) &&
        !currentUser.Roles.Contains("Owner", StringComparer.Ordinal))
    {
      return Result.Failure<InventorySettingsResponse>(SettingsErrors.Forbidden);
    }

    if (command.DefaultLowStockThreshold < 0)
    {
      return Result.Failure<InventorySettingsResponse>(SettingsErrors.InvalidThreshold);
    }

    var businessId = new BusinessId(businessIdValue);
    var now = clock.UtcNow;
    var existing = await repository.GetAsync(businessId, cancellationToken);

    if (existing is null)
    {
      existing = new InventorySettings(
        businessId, command.EnableLowStockAlerts, command.DefaultLowStockThreshold,
        command.RequireReasonForInventoryAdjustment,
        command.AllowInventoryTransferBetweenBranches, userId, now);
    }
    else
    {
      existing.Update(
        command.EnableLowStockAlerts, command.DefaultLowStockThreshold,
        command.RequireReasonForInventoryAdjustment,
        command.AllowInventoryTransferBetweenBranches, userId, now);
    }

    await repository.UpsertAsync(existing, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    await auditLog.WriteAsync(
      new AuditEntry(
        businessId, userId, AuditActionType.InventorySettingsUpdated,
        AuditEntityType.Settings, null, "Inventory settings updated."),
      cancellationToken);

    return Result.Success(ToResponse(existing));
  }

  private static InventorySettingsResponse ToResponse(InventorySettings s)
    => new(s.BusinessId.Value, s.EnableLowStockAlerts, s.DefaultLowStockThreshold,
        s.RequireReasonForInventoryAdjustment, s.AllowInventoryTransferBetweenBranches,
        s.UpdatedBy, s.UpdatedAt);
}
