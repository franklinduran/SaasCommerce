using System.Reflection;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.Modules;
using SaasCommerce.SharedKernel;
using FluentAssertions;
using NetArchTest.Rules;

namespace SaasCommerce.Architecture.Tests;

public sealed class ArchitectureDependencyTests
{
  private static readonly Assembly SharedKernelAssembly = SharedKernelAssemblyReference.Assembly;
  private static readonly Assembly BuildingBlocksAssembly = BuildingBlocksAssemblyReference.Assembly;
  private static readonly Assembly ModulesAssembly = ModulesAssemblyReference.Assembly;

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
}
