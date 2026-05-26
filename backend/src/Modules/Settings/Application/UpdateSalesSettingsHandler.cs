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

public sealed record UpdateSalesSettingsCommand(
  bool AllowNegativeStock,
  bool AllowDiscounts,
  bool RequireCustomerForCreditSale,
  string? DefaultPaymentMethod,
  bool EnableReceiptPrintAfterSale,
  bool EnableInvoiceAutoGeneration);

public sealed class UpdateSalesSettingsHandler(
  ISalesSettingsRepository repository,
  ICurrentUserService currentUser,
  IAuditLogWriter auditLog,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<SalesSettingsResponse>> Handle(
    UpdateSalesSettingsCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<SalesSettingsResponse>> HandleCoreAsync(
    UpdateSalesSettingsCommand command,
    CancellationToken cancellationToken)
  {
    if (!currentUser.IsAuthenticated ||
        currentUser.BusinessId is not { } businessIdValue ||
        currentUser.UserId is not { } userId)
    {
      return Result.Failure<SalesSettingsResponse>(SettingsErrors.UserContextRequired);
    }

    if (!currentUser.Roles.Contains("Admin", StringComparer.Ordinal) &&
        !currentUser.Roles.Contains("Owner", StringComparer.Ordinal))
    {
      return Result.Failure<SalesSettingsResponse>(SettingsErrors.Forbidden);
    }

    var businessId = new BusinessId(businessIdValue);
    var now = clock.UtcNow;
    var existing = await repository.GetAsync(businessId, cancellationToken);

    if (existing is null)
    {
      existing = new SalesSettings(ToDetails(command, businessId, userId, now));
    }
    else
    {
      existing.Update(ToDetails(command, businessId, userId, now));
    }

    await repository.UpsertAsync(existing, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    await auditLog.WriteAsync(
      new AuditEntry(
        businessId, userId, AuditActionType.SalesSettingsUpdated,
        AuditEntityType.Settings, null, "Sales settings updated."),
      cancellationToken);

    return Result.Success(ToResponse(existing));
  }

  private static SalesSettingsResponse ToResponse(SalesSettings s)
    => new(s.BusinessId.Value, s.AllowNegativeStock, s.AllowDiscounts,
        s.RequireCustomerForCreditSale, s.DefaultPaymentMethod,
        s.EnableReceiptPrintAfterSale, s.EnableInvoiceAutoGeneration,
        s.UpdatedBy, s.UpdatedAt);

  private static SalesSettingsDetails ToDetails(
    UpdateSalesSettingsCommand command,
    BusinessId businessId,
    Guid userId,
    DateTimeOffset now)
    => new()
    {
      BusinessId = businessId,
      AllowNegativeStock = command.AllowNegativeStock,
      AllowDiscounts = command.AllowDiscounts,
      RequireCustomerForCreditSale = command.RequireCustomerForCreditSale,
      DefaultPaymentMethod = command.DefaultPaymentMethod,
      EnableReceiptPrintAfterSale = command.EnableReceiptPrintAfterSale,
      EnableInvoiceAutoGeneration = command.EnableInvoiceAutoGeneration,
      UpdatedBy = userId,
      UpdatedAt = now
    };
}
