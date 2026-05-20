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

public sealed record UpdateBillingSettingsCommand(
  string? ReceiptHeaderText,
  string? ReceiptFooterText,
  bool ShowLogoOnReceipt,
  bool ShowRncOnReceipt,
  bool EnableInvoiceAutoGeneration,
  string InvoicePrefix,
  int InvoiceSequenceStart);

public sealed class UpdateBillingSettingsHandler(
  IBillingSettingsRepository repository,
  ICurrentUserService currentUser,
  IAuditLogWriter auditLog,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<BillingSettingsResponse>> Handle(
    UpdateBillingSettingsCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<BillingSettingsResponse>> HandleCoreAsync(
    UpdateBillingSettingsCommand command,
    CancellationToken cancellationToken)
  {
    if (!currentUser.IsAuthenticated ||
        currentUser.BusinessId is not { } businessIdValue ||
        currentUser.UserId is not { } userId)
    {
      return Result.Failure<BillingSettingsResponse>(SettingsErrors.UserContextRequired);
    }

    if (!currentUser.Roles.Contains("Admin", StringComparer.Ordinal) &&
        !currentUser.Roles.Contains("Owner", StringComparer.Ordinal))
    {
      return Result.Failure<BillingSettingsResponse>(SettingsErrors.Forbidden);
    }

    if (command.InvoiceSequenceStart < 1)
    {
      return Result.Failure<BillingSettingsResponse>(SettingsErrors.InvalidSequenceStart);
    }

    var businessId = new BusinessId(businessIdValue);
    var now = clock.UtcNow;
    var existing = await repository.GetAsync(businessId, cancellationToken);

    if (existing is null)
    {
      existing = new BillingSettings(
        businessId, command.ReceiptHeaderText, command.ReceiptFooterText,
        command.ShowLogoOnReceipt, command.ShowRncOnReceipt,
        command.EnableInvoiceAutoGeneration, command.InvoicePrefix,
        command.InvoiceSequenceStart, userId, now);
    }
    else
    {
      existing.Update(
        command.ReceiptHeaderText, command.ReceiptFooterText,
        command.ShowLogoOnReceipt, command.ShowRncOnReceipt,
        command.EnableInvoiceAutoGeneration, command.InvoicePrefix,
        command.InvoiceSequenceStart, userId, now);
    }

    await repository.UpsertAsync(existing, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    await auditLog.WriteAsync(
      new AuditEntry(
        businessId, userId, AuditActionType.BillingSettingsUpdated,
        AuditEntityType.Settings, null, "Billing settings updated."),
      cancellationToken);

    return Result.Success(ToResponse(existing));
  }

  private static BillingSettingsResponse ToResponse(BillingSettings s)
    => new(s.BusinessId.Value, s.ReceiptHeaderText, s.ReceiptFooterText,
        s.ShowLogoOnReceipt, s.ShowRncOnReceipt, s.EnableInvoiceAutoGeneration,
        s.InvoicePrefix, s.InvoiceSequenceStart, s.UpdatedBy, s.UpdatedAt);
}
