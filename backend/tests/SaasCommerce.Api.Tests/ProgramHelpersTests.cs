#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SaasCommerce.Modules.Identity.Contracts;

namespace SaasCommerce.Api.Tests;

public sealed class ProgramHelpersTests
{
  // ── GetCorsAllowedOrigins ────────────────────────────────────────────────

  [Fact]
  public void GetCorsAllowedOrigins_ShouldReturnDefaults_WhenNotConfigured()
  {
    var configuration = new ConfigurationBuilder().Build();

    var origins = ProgramHelpers.GetCorsAllowedOrigins(configuration);

    origins.Should().BeEquivalentTo(["http://localhost:5173", "http://127.0.0.1:5173"]);
  }

  [Fact]
  public void GetCorsAllowedOrigins_ShouldReturnConfiguredOrigins_WhenSet()
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Cors:AllowedOrigins:0"] = "https://app.example.com",
        ["Cors:AllowedOrigins:1"] = "https://admin.example.com"
      })
      .Build();

    var origins = ProgramHelpers.GetCorsAllowedOrigins(configuration);

    origins.Should().BeEquivalentTo(["https://app.example.com", "https://admin.example.com"]);
  }

  // ── GetAllSystemPermissions ──────────────────────────────────────────────

  [Fact]
  public void GetAllSystemPermissions_ShouldReturnNonEmpty()
  {
    var permissions = ProgramHelpers.GetAllSystemPermissions().ToList();

    permissions.Should().NotBeEmpty();
  }

  [Fact]
  public void GetAllSystemPermissions_ShouldContainKnownPermissions()
  {
    var permissions = ProgramHelpers.GetAllSystemPermissions().ToHashSet();

    permissions.Should().Contain(SystemPermissions.DashboardView);
    permissions.Should().Contain(SystemPermissions.UsersView);
    permissions.Should().Contain(SystemPermissions.ReportsView);
  }

  // ── BuildHealthReadyResponse ─────────────────────────────────────────────

  [Fact]
  public void BuildHealthReadyResponse_ShouldReturnHealthy_WhenAllDependenciesOk()
  {
    var response = ProgramHelpers.BuildHealthReadyResponse(
      databaseReady: true,
      rabbitMqReady: true,
      outboxHealthy: true,
      outboxStaleCount: 0);

    response.Status.Should().Be("Healthy");
    response.PostgreSql.Status.Should().Be("Healthy");
    response.RabbitMq.Status.Should().Be("Healthy");
    response.Outbox.Status.Should().Be("Healthy");
  }

  [Fact]
  public void BuildHealthReadyResponse_ShouldReturnDegraded_WhenOutboxHasStaleMessages()
  {
    var response = ProgramHelpers.BuildHealthReadyResponse(
      databaseReady: true,
      rabbitMqReady: true,
      outboxHealthy: false,
      outboxStaleCount: 5);

    response.Status.Should().Be("Degraded");
    response.Outbox.Status.Should().Contain("5 stale");
  }

  [Fact]
  public void BuildHealthReadyResponse_ShouldReturnUnhealthy_WhenDatabaseIsDown()
  {
    var response = ProgramHelpers.BuildHealthReadyResponse(
      databaseReady: false,
      rabbitMqReady: true,
      outboxHealthy: true,
      outboxStaleCount: 0);

    response.Status.Should().Be("Unhealthy");
    response.PostgreSql.Status.Should().Be("Unhealthy");
    response.RabbitMq.Status.Should().Be("Healthy");
  }

  [Fact]
  public void BuildHealthReadyResponse_ShouldReturnUnhealthy_WhenRabbitMqIsDown()
  {
    var response = ProgramHelpers.BuildHealthReadyResponse(
      databaseReady: true,
      rabbitMqReady: false,
      outboxHealthy: true,
      outboxStaleCount: 0);

    response.Status.Should().Be("Unhealthy");
    response.RabbitMq.Status.Should().Be("Unhealthy");
  }

  // ── CanConnectToRabbitMqAsync ─────────────────────────────────────────────

  [Fact]
  public async Task CanConnectToRabbitMqAsync_ShouldReturnTrue_WhenUseInMemoryIsTrue()
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["RabbitMq:UseInMemory"] = "true"
      })
      .Build();

    var result = await ProgramHelpers.CanConnectToRabbitMqAsync(configuration, CancellationToken.None);

    result.Should().BeTrue();
  }

  [Fact]
  public async Task CanConnectToRabbitMqAsync_ShouldReturnFalse_WhenHostIsUnreachable()
  {
    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["RabbitMq:UseInMemory"] = "false",
        ["RabbitMq:Host"] = "unreachable.invalid",
        ["RabbitMq:Port"] = "5672"
      })
      .Build();

    var result = await ProgramHelpers.CanConnectToRabbitMqAsync(configuration, CancellationToken.None);

    result.Should().BeFalse();
  }
}

#pragma warning restore CA1707
