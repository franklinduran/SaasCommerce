namespace SaasCommerce.Api;

internal sealed record HealthReadyResponse(
  string Status,
  HealthDependencyStatus PostgreSql,
  HealthDependencyStatus RabbitMq);

internal sealed record HealthDependencyStatus(string Name, string Status);
