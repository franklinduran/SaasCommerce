using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Billing.Contracts.Events.V1;

/// <summary>
/// Event published when a business subscription changes (plan change, activation, cancellation, etc.)
/// </summary>
public sealed record BusinessSubscriptionChangedEventV1(
  Guid BusinessId,
  Guid SubscriptionId,
  Guid? PreviousPlanId,
  Guid NewPlanId,
  string PreviousStatus,
  string NewStatus,
  Guid? ChangedByUserId,
  string Reason,
  DateTimeOffset ChangedAt) : IIntegrationEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public Guid CorrelationId { get; } = Guid.NewGuid();
  public int Version { get; } = 1;
  public DateTimeOffset OccurredAt => ChangedAt;
}

/// <summary>
/// Event published when a business reaches a subscription limit
/// </summary>
public sealed record SubscriptionLimitReachedEventV1(
  Guid BusinessId,
  string Feature,
  int CurrentUsage,
  int MaxAllowed,
  DateTimeOffset ReachedAt) : IIntegrationEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public Guid CorrelationId { get; } = Guid.NewGuid();
  public int Version { get; } = 1;
  public DateTimeOffset OccurredAt => ReachedAt;
}

/// <summary>
/// Event published when a subscription trial is about to expire (within 7 days)
/// </summary>
public sealed record SubscriptionTrialAboutToExpireEventV1(
  Guid BusinessId,
  Guid SubscriptionId,
  DateTimeOffset TrialEndsAt,
  DateTimeOffset AlertSentAt) : IIntegrationEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public Guid CorrelationId { get; } = Guid.NewGuid();
  public int Version { get; } = 1;
  public DateTimeOffset OccurredAt => AlertSentAt;
}

/// <summary>
/// Event published when a subscription period is about to expire
/// </summary>
public sealed record SubscriptionPeriodAboutToExpireEventV1(
  Guid BusinessId,
  Guid SubscriptionId,
  DateTimeOffset PeriodEndsAt,
  DateTimeOffset AlertSentAt) : IIntegrationEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public Guid CorrelationId { get; } = Guid.NewGuid();
  public int Version { get; } = 1;
  public DateTimeOffset OccurredAt => AlertSentAt;
}
