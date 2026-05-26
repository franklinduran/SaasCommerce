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

public sealed record UpdateBusinessSettingsCommand(
  string? CommercialName,
  string? LegalName,
  string? Rnc,
  string? Phone,
  string? Email,
  string? Address,
  string Currency,
  string Timezone,
  string? LogoUrl,
  string? ReceiptFooterText);

public sealed class UpdateBusinessSettingsHandler(
  IBusinessSettingsRepository repository,
  ICurrentUserService currentUser,
  IAuditLogWriter auditLog,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<BusinessSettingsResponse>> Handle(
    UpdateBusinessSettingsCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<BusinessSettingsResponse>> HandleCoreAsync(
    UpdateBusinessSettingsCommand command,
    CancellationToken cancellationToken)
  {
    if (!currentUser.IsAuthenticated ||
        currentUser.BusinessId is not { } businessIdValue ||
        currentUser.UserId is not { } userId)
    {
      return Result.Failure<BusinessSettingsResponse>(SettingsErrors.UserContextRequired);
    }

    if (!currentUser.Roles.Contains("Admin", StringComparer.Ordinal) &&
        !currentUser.Roles.Contains("Owner", StringComparer.Ordinal))
    {
      return Result.Failure<BusinessSettingsResponse>(SettingsErrors.Forbidden);
    }

    if (!IsValidCurrency(command.Currency))
    {
      return Result.Failure<BusinessSettingsResponse>(SettingsErrors.InvalidCurrency);
    }

    if (!string.IsNullOrWhiteSpace(command.Email) && !IsValidEmail(command.Email))
    {
      return Result.Failure<BusinessSettingsResponse>(SettingsErrors.InvalidEmail);
    }

    var businessId = new BusinessId(businessIdValue);
    var now = clock.UtcNow;
    var existing = await repository.GetAsync(businessId, cancellationToken);

    if (existing is null)
    {
      existing = new BusinessSettings(ToDetails(command, businessId, userId, now));
    }
    else
    {
      existing.Update(ToDetails(command, businessId, userId, now));
    }

    await repository.UpsertAsync(existing, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    await auditLog.WriteAsync(
      new AuditEntry(
        businessId, userId, AuditActionType.BusinessSettingsUpdated,
        AuditEntityType.Settings, null, "Business settings updated."),
      cancellationToken);

    return Result.Success(ToResponse(existing));
  }

  private static bool IsValidCurrency(string? currency)
    => !string.IsNullOrWhiteSpace(currency) && currency.Trim().Length == 3;

  private static BusinessSettingsDetails ToDetails(
    UpdateBusinessSettingsCommand command,
    BusinessId businessId,
    Guid userId,
    DateTimeOffset now)
    => new()
    {
      BusinessId = businessId,
      CommercialName = command.CommercialName,
      LegalName = command.LegalName,
      Rnc = command.Rnc,
      Phone = command.Phone,
      Email = command.Email,
      Address = command.Address,
      Currency = command.Currency,
      Timezone = command.Timezone,
      LogoUrl = command.LogoUrl,
      ReceiptFooterText = command.ReceiptFooterText,
      UpdatedBy = userId,
      UpdatedAt = now
    };

  private static bool IsValidEmail(string email)
  {
    try
    {
      var addr = new System.Net.Mail.MailAddress(email);
      return string.Equals(addr.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
    }
    catch
    {
      return false;
    }
  }

  private static BusinessSettingsResponse ToResponse(BusinessSettings s)
    => new(s.BusinessId.Value, s.CommercialName, s.LegalName, s.Rnc, s.Phone,
        s.Email, s.Address, s.Currency, s.Timezone, s.LogoUrl,
        s.ReceiptFooterText, s.UpdatedBy, s.UpdatedAt);
}
