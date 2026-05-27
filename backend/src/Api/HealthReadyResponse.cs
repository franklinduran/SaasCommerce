namespace SaasCommerce.Api;

internal sealed record HealthReadyResponse(
  string Status,
  HealthDependencyStatus PostgreSql,
  HealthDependencyStatus RabbitMq,
  HealthDependencyStatus Outbox);

internal sealed record HealthDependencyStatus(string Name, string Status);
