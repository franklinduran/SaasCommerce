using FluentAssertions;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Modules.Tests;

public sealed class OperationalNotificationDomainTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 16, 0, 0, TimeSpan.Zero);

  [Fact]
  public void Create_ShouldInitializeUnreadNotification()
  {
    var relatedId = Guid.NewGuid();
    var notification = OperationalNotification.Create(new OperationalNotificationDraft
    {
      Id = Guid.NewGuid(),
      BusinessId = new BusinessId(Guid.NewGuid()),
      BranchId = Guid.NewGuid(),
      Type = OperationalNotificationType.LowStock,
      Severity = OperationalNotificationSeverity.Warning,
      Title = "Stock bajo",
      Message = "Quedan pocas unidades.",
      RelatedEntityId = relatedId,
      RelatedEntityType = "Product",
      CreatedAt = Now
    });

    notification.Status.Should().Be(OperationalNotificationStatus.Unread);
    notification.Title.Should().Be("Stock bajo");
    notification.Message.Should().Be("Quedan pocas unidades.");
    notification.RelatedEntityId.Should().Be(relatedId);
    notification.RelatedEntityType.Should().Be("Product");
    notification.ReadAt.Should().BeNull();
    notification.ReadByUserId.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldRejectEmptyId()
  {
    FluentActions.Invoking(() => OperationalNotification.Create(new OperationalNotificationDraft
      {
        Id = Guid.Empty,
        BusinessId = new BusinessId(Guid.NewGuid()),
        Type = OperationalNotificationType.CustomerDebtOverdue,
        Severity = OperationalNotificationSeverity.Critical,
        Title = "Pago vencido",
        Message = "Revisar factura.",
        CreatedAt = Now
      }))
      .Should().Throw<ArgumentException>();
  }

  [Fact]
  public void MarkRead_ShouldBeIdempotent()
  {
    var userId = Guid.NewGuid();
    var notification = OperationalNotification.Create(new OperationalNotificationDraft
    {
      Id = Guid.NewGuid(),
      BusinessId = new BusinessId(Guid.NewGuid()),
      Type = OperationalNotificationType.InvoiceFailed,
      Severity = OperationalNotificationSeverity.Info,
      Title = "Transferencia",
      Message = "Procesada.",
      CreatedAt = Now
    });

    notification.MarkRead(userId, Now.AddMinutes(1));
    notification.MarkRead(Guid.NewGuid(), Now.AddMinutes(2));

    notification.Status.Should().Be(OperationalNotificationStatus.Read);
    notification.ReadAt.Should().Be(Now.AddMinutes(1));
    notification.ReadByUserId.Should().Be(userId);

    FluentActions.Invoking(() => notification.MarkRead(Guid.Empty, Now))
      .Should().Throw<ArgumentException>();
  }
}

#pragma warning restore CA1707
