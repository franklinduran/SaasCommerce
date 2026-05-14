using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules;
using FluentAssertions;

namespace SaasCommerce.Modules.Tests;

public sealed class IntegrationEventContractTests
{
  [Fact]
  public void IntegrationEventsShouldExposeRequiredMetadata()
  {
    var eventTypes = ModulesAssemblyReference.Assembly
      .GetTypes()
      .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type) && !type.IsInterface)
      .ToArray();

    eventTypes.Should().NotBeEmpty();

    foreach (var eventType in eventTypes)
    {
      AssertProperty(eventType, nameof(IIntegrationEvent.EventId), typeof(Guid));
      AssertProperty(eventType, nameof(IIntegrationEvent.CorrelationId), typeof(Guid));
      AssertProperty(eventType, nameof(IIntegrationEvent.BusinessId), typeof(Guid));
      AssertProperty(eventType, nameof(IIntegrationEvent.OccurredAt), typeof(DateTimeOffset));
      AssertProperty(eventType, nameof(IIntegrationEvent.Version), typeof(int));
    }
  }

  private static void AssertProperty(Type eventType, string propertyName, Type propertyType)
  {
    var property = eventType.GetProperty(propertyName);

    property.Should().NotBeNull();
    property!.PropertyType.Should().Be(propertyType);
  }
}
