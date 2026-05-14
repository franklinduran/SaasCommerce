using System.Reflection;
using System.Runtime.CompilerServices;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.Modules;
using FluentAssertions;

namespace SaasCommerce.Modules.Tests;

public sealed class IntegrationEventContractTests
{
  [Fact]
  public void IntegrationEventTypesShouldImplementContract()
  {
    var eventTypes = AllContractAssemblies()
      .SelectMany(assembly => assembly.GetTypes())
      .Where(type => IsIntegrationEventTypeName(type) && !type.IsInterface)
      .ToArray();

    eventTypes.Should().NotBeEmpty();

    foreach (var eventType in eventTypes)
    {
      typeof(IIntegrationEvent)
        .IsAssignableFrom(eventType)
        .Should()
        .BeTrue($"{eventType.FullName} must implement IIntegrationEvent");
    }
  }

  [Fact]
  public void IntegrationEventsShouldExposeRequiredMetadata()
  {
    var eventTypes = GetIntegrationEventTypes();

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

  [Fact]
  public void IntegrationEventsShouldLiveInsideVersionedContractsNamespace()
  {
    var eventTypes = GetIntegrationEventTypes();

    foreach (var eventType in eventTypes)
    {
      eventType.Namespace.Should().NotBeNull();
      eventType.Namespace!.Should().Contain(".Contracts.Events.V");
      eventType.Name.Should().EndWith("V1");
    }
  }

  [Fact]
  public void IntegrationEventsShouldBeImmutableRecordLikeTypes()
  {
    var eventTypes = GetIntegrationEventTypes();

    foreach (var eventType in eventTypes)
    {
      eventType.IsSealed.Should().BeTrue($"{eventType.FullName} should be sealed");
      eventType
        .GetConstructors()
        .Should()
        .Contain(constructor => constructor.GetParameters().Length > 0);

      foreach (var property in eventType.GetProperties())
      {
        IsInitOnlyOrReadOnly(property)
          .Should()
          .BeTrue($"{eventType.FullName}.{property.Name} should be immutable");
      }
    }
  }

  private static void AssertProperty(Type eventType, string propertyName, Type propertyType)
  {
    var property = eventType.GetProperty(propertyName);

    property.Should().NotBeNull();
    property!.PropertyType.Should().Be(propertyType);
  }

  private static Type[] GetIntegrationEventTypes()
    => AllContractAssemblies()
      .SelectMany(assembly => assembly.GetTypes())
      .Where(type => typeof(IIntegrationEvent).IsAssignableFrom(type) && !type.IsInterface)
      .ToArray();

  private static Assembly[] AllContractAssemblies()
    => [BuildingBlocksAssemblyReference.Assembly, ModulesAssemblyReference.Assembly];

  private static bool IsIntegrationEventTypeName(Type type)
    => type.Name.Contains("IntegrationEvent", StringComparison.Ordinal);

  private static bool IsInitOnlyOrReadOnly(PropertyInfo property)
  {
    var setMethod = property.SetMethod;

    if (setMethod is null)
    {
      return true;
    }

    return setMethod
      .ReturnParameter
      .GetRequiredCustomModifiers()
      .Contains(typeof(IsExternalInit));
  }
}
