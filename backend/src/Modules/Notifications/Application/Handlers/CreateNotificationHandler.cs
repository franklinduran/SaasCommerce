using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Notifications.Application.Abstractions;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Notifications.Application.Handlers;

/// <summary>
/// Internal command used by Worker consumers to persist a new notification.
/// BusinessId comes from the integration event — NOT from user context.
/// </summary>
public sealed record CreateNotificationCommand(
  Guid BusinessId,
  Guid? BranchId,
  OperationalNotificationType Type,
  OperationalNotificationSeverity Severity,
  string Title,
  string Message,
  Guid? RelatedEntityId = null,
  string? RelatedEntityType = null);

public sealed class CreateNotificationHandler(
  IOperationalNotificationRepository repository,
  IClock clock)
{
  public async Task<Result<Guid>> Handle(
    CreateNotificationCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var notification = OperationalNotification.Create(new OperationalNotificationDraft
    {
      Id = Guid.NewGuid(),
      BusinessId = new BusinessId(command.BusinessId),
      BranchId = command.BranchId,
      Type = command.Type,
      Severity = command.Severity,
      Title = command.Title,
      Message = command.Message,
      RelatedEntityId = command.RelatedEntityId,
      RelatedEntityType = command.RelatedEntityType,
      CreatedAt = clock.UtcNow
    });

    await repository.AddAsync(notification, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    return Result.Success(notification.Id);
  }
}
