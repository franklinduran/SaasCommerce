using System.Reflection;
using FluentAssertions;
using MassTransit;
using NetArchTest.Rules;
using SaasCommerce.Api;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules;
using SaasCommerce.SharedKernel;
using SaasCommerce.Worker;

namespace SaasCommerce.Architecture.Tests;

public sealed class ArchitectureDependencyTests
{
  private static readonly Assembly SharedKernelAssembly = SharedKernelAssemblyReference.Assembly;
  private static readonly Assembly BuildingBlocksAssembly = BuildingBlocksAssemblyReference.Assembly;
  private static readonly Assembly ModulesAssembly = ModulesAssemblyReference.Assembly;
  private static readonly Assembly ApiAssembly = ApiAssemblyReference.Assembly;
  private static readonly Assembly WorkerAssembly = WorkerAssemblyReference.Assembly;

  private static readonly string[] ModuleNames =
  [
    "Tenancy",
    "Identity",
    "Catalog",
    "Inventory",
    "Sales",
    "Customers",
    "Billing",
    "Payments",
    "Purchasing",
    "Reporting"
  ];

  [Theory]
  [InlineData("SaasCommerce.BuildingBlocks")]
  [InlineData("SaasCommerce.Modules")]
  [InlineData("MassTransit")]
  [InlineData("Microsoft.EntityFrameworkCore")]
  [InlineData("Microsoft.AspNetCore")]
  public void SharedKernelShouldNotDependOnForbiddenNamespaces(string dependency)
  {
    AssertNoDependency(SharedKernelAssembly, dependency);
  }

  [Theory]
  [InlineData("SaasCommerce.BuildingBlocks.Infrastructure")]
  [InlineData("Microsoft.EntityFrameworkCore")]
  [InlineData("MassTransit")]
  [InlineData("Microsoft.AspNetCore.SignalR")]
  public void BuildingBlocksApplicationShouldNotDependOnTechnicalDetails(string dependency)
  {
    AssertNoDependency(
      BuildingBlocksAssembly,
      "SaasCommerce.BuildingBlocks.Application",
      dependency);
  }

  [Theory]
  [InlineData("SaasCommerce.Api")]
  [InlineData("SaasCommerce.Worker")]
  public void CoreAssembliesShouldNotDependOnEntrypoints(string dependency)
  {
    AssertNoDependency(SharedKernelAssembly, dependency);
    AssertNoDependency(BuildingBlocksAssembly, dependency);
    AssertNoDependency(ModulesAssembly, dependency);
  }

  [Theory]
  [InlineData("MassTransit")]
  [InlineData("Microsoft.EntityFrameworkCore")]
  [InlineData("Microsoft.AspNetCore")]
  public void ModuleDomainsShouldNotDependOnTechnicalFrameworks(string dependency)
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Domain",
        dependency);
    }
  }

  [Fact]
  public void ModuleDomainsShouldNotDependOnApplicationContractsOrInfrastructure()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Domain",
        $"SaasCommerce.Modules.{moduleName}.Application");
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Domain",
        $"SaasCommerce.Modules.{moduleName}.Contracts");
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Domain",
        $"SaasCommerce.Modules.{moduleName}.Infrastructure");
    }
  }

  [Fact]
  public void ModuleApplicationsShouldNotDependOnInfrastructure()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Application",
        $"SaasCommerce.Modules.{moduleName}.Infrastructure");
    }
  }

  [Fact]
  public void ApplicationShouldNotReferenceMassTransit()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Application",
        "MassTransit");
    }
  }

  [Fact]
  public void ApplicationShouldNotReferenceSignalR()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Application",
        "Microsoft.AspNetCore.SignalR");
    }

    AssertNoDependency(
      BuildingBlocksAssembly,
      "SaasCommerce.BuildingBlocks.Application",
      "Microsoft.AspNetCore.SignalR");
  }

  [Fact]
  public void DomainShouldNotReferenceInfrastructureOrEntityFramework()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Domain",
        "Infrastructure");
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Domain",
        "Microsoft.EntityFrameworkCore");
    }
  }

  [Fact]
  public void WorkerAndApiShouldNotBeReferencedByApplication()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Application",
        "SaasCommerce.Worker");
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Application",
        "SaasCommerce.Api");
    }

    AssertNoDependency(
      BuildingBlocksAssembly,
      "SaasCommerce.BuildingBlocks.Application",
      "SaasCommerce.Worker");
    AssertNoDependency(
      BuildingBlocksAssembly,
      "SaasCommerce.BuildingBlocks.Application",
      "SaasCommerce.Api");
  }

  [Fact]
  public void ModuleDomainsShouldNotDependOnOtherModuleDomains()
  {
    foreach (var sourceModule in ModuleNames)
    {
      foreach (var targetModule in ModuleNames.Where(module => module != sourceModule))
      {
        AssertNoDependency(
          ModulesAssembly,
          $"SaasCommerce.Modules.{sourceModule}.Domain",
          $"SaasCommerce.Modules.{targetModule}.Domain");
      }
    }
  }

  [Fact]
  public void ModulesShouldNotDependOnOtherModuleInfrastructure()
  {
    foreach (var sourceModule in ModuleNames)
    {
      foreach (var targetModule in ModuleNames.Where(module => module != sourceModule))
      {
        AssertNoDependency(
          ModulesAssembly,
          $"SaasCommerce.Modules.{sourceModule}",
          $"SaasCommerce.Modules.{targetModule}.Infrastructure");
      }
    }
  }

  [Fact]
  public void ModuleContractsShouldNotDependOnInfrastructure()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(
        ModulesAssembly,
        $"SaasCommerce.Modules.{moduleName}.Contracts",
        "Infrastructure");
    }
  }

  [Fact]
  public void GlobalApplicationProjectShouldNotExist()
  {
    var repositoryRoot = FindRepositoryRoot();

    Directory
      .Exists(Path.Combine(repositoryRoot, "backend", "src", "SaasCommerce.Application"))
      .Should()
      .BeFalse("use cases must live under backend/src/Modules/module/Application");
  }

  [Fact]
  public void ApiShouldNotDependOnModuleDomainOrInfrastructure()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(ApiAssembly, $"SaasCommerce.Modules.{moduleName}.Domain");
      AssertNoDependency(ApiAssembly, $"SaasCommerce.Modules.{moduleName}.Infrastructure");
    }
  }

  [Fact]
  public void WorkerShouldNotDependOnModuleDomainOrInfrastructure()
  {
    foreach (var moduleName in ModuleNames)
    {
      AssertNoDependency(WorkerAssembly, $"SaasCommerce.Modules.{moduleName}.Domain");
      AssertNoDependency(WorkerAssembly, $"SaasCommerce.Modules.{moduleName}.Infrastructure");
    }
  }

  [Fact]
  public void WorkerConsumersShouldUseIdempotentConsumerBase()
  {
    var consumerTypes = WorkerAssembly
      .GetTypes()
      .Where(type => !type.IsAbstract && ImplementsGeneric(type, typeof(IConsumer<>)))
      .ToArray();

    consumerTypes.Should().NotBeEmpty();

    foreach (var consumerType in consumerTypes)
    {
      InheritsGeneric(consumerType, typeof(IdempotentConsumer<>))
        .Should()
        .BeTrue($"{consumerType.FullName} should be idempotent");
    }
  }

  [Fact]
  public void UseCaseFilesShouldLiveInsideModuleApplications()
  {
    var repositoryRoot = FindRepositoryRoot();
    var backendSource = Path.Combine(repositoryRoot, "backend", "src");
    var useCaseFiles = Directory
      .EnumerateFiles(backendSource, "*.cs", SearchOption.AllDirectories)
      .Where(path => !IsGeneratedPath(path))
      .Where(path =>
        path.EndsWith("Command.cs", StringComparison.Ordinal) ||
        path.EndsWith("Query.cs", StringComparison.Ordinal) ||
        path.EndsWith("Handler.cs", StringComparison.Ordinal))
      .ToArray();

    foreach (var file in useCaseFiles)
    {
      file
        .Replace(Path.DirectorySeparatorChar, '/')
        .Should()
        .Contain("/backend/src/Modules/")
        .And
        .Contain("/Application/");
    }
  }

  private static void AssertNoDependency(Assembly assembly, string dependency)
  {
    var result = Types
      .InAssembly(assembly)
      .Should()
      .NotHaveDependencyOn(dependency)
      .GetResult();

    result.IsSuccessful.Should().BeTrue(
      $"{assembly.GetName().Name} should not depend on {dependency}");
  }

  private static void AssertNoDependency(
    Assembly assembly,
    string sourceNamespace,
    string dependency)
  {
    var result = Types
      .InAssembly(assembly)
      .That()
      .ResideInNamespace(sourceNamespace)
      .Should()
      .NotHaveDependencyOn(dependency)
      .GetResult();

    result.IsSuccessful.Should().BeTrue(
      $"{sourceNamespace} should not depend on {dependency}");
  }

  private static string FindRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SaasCommerce.slnx")))
    {
      directory = directory.Parent;
    }

    directory.Should().NotBeNull();

    return directory!.FullName;
  }

  private static bool ImplementsGeneric(Type type, Type genericType)
    => type
      .GetInterfaces()
      .Any(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == genericType);

  private static bool InheritsGeneric(Type type, Type genericType)
  {
    for (var current = type.BaseType; current is not null; current = current.BaseType)
    {
      if (current.IsGenericType && current.GetGenericTypeDefinition() == genericType)
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsGeneratedPath(string path)
    => path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
       path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}
