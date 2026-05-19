using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests.Identity;

public sealed class AuditLogTests
{
  [Fact]
  public void ConstructorShouldSetAllProperties()
  {
    var id = Guid.NewGuid();
    var businessId = new BusinessId(Guid.NewGuid());
    var userId = Guid.NewGuid();
    var entityId = Guid.NewGuid();
    var now = DateTimeOffset.UtcNow;
    var entry = new AuditEntry(businessId, userId, "sale.cancelled", "Sale", entityId, "Test description", "192.168.1.1");

    var log = new AuditLog(id, entry, now);

    log.Id.Should().Be(id);
    log.BusinessId.Should().Be(businessId);
    log.UserId.Should().Be(userId);
    log.Action.Should().Be("sale.cancelled");
    log.EntityName.Should().Be("Sale");
    log.Description.Should().Be("Test description");
    log.IpAddress.Should().Be("192.168.1.1");
    log.CreatedAt.Should().Be(now);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void ConstructorShouldRejectInvalidAction(string? action)
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var entry = new AuditEntry(businessId, Guid.NewGuid(), action!, "Sale", null);

    var act = () => new AuditLog(Guid.NewGuid(), entry, DateTimeOffset.UtcNow);

    act.Should().Throw<ArgumentException>();
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void ConstructorShouldRejectInvalidEntityName(string? entityName)
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var entry = new AuditEntry(businessId, Guid.NewGuid(), "test.action", entityName!, null);

    var act = () => new AuditLog(Guid.NewGuid(), entry, DateTimeOffset.UtcNow);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void OptionalFieldsCanBeNull()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var entry = new AuditEntry(businessId, null, "test.action", "TestEntity", null);

    var log = new AuditLog(Guid.NewGuid(), entry, DateTimeOffset.UtcNow);

    log.UserId.Should().BeNull();
    log.EntityId.Should().BeNull();
    log.Description.Should().BeNull();
    log.IpAddress.Should().BeNull();
  }
}
