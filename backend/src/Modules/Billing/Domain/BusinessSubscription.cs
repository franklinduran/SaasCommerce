using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Domain;

/// <summary>
/// Represents a business's current subscription state.
/// Links a business to a plan and tracks trial/billing period dates.
/// One subscription per business at any given time.
/// </summary>
public sealed class BusinessSubscription
{
  private BusinessSubscription()
  {
  }

  private BusinessSubscription(BusinessSubscriptionState state)
  {
    if (state.Id == Guid.Empty)
    {
      throw new ArgumentException("Subscription id cannot be empty.", nameof(state));
    }

    if (state.PlanId == Guid.Empty)
    {
      throw new ArgumentException("Plan id cannot be empty.", nameof(state));
    }

    ValidateStatusDates(
      state.Status,
      state.TrialEndsAt,
      state.CurrentPeriodStart,
      state.CurrentPeriodEnd,
      state.CancelledAt);

    Id = state.Id;
    BusinessId = state.BusinessId;
    PlanId = state.PlanId;
    Status = state.Status;
    StartedAt = state.StartedAt;
    TrialEndsAt = state.TrialEndsAt;
    CurrentPeriodStart = state.CurrentPeriodStart;
    CurrentPeriodEnd = state.CurrentPeriodEnd;
    CancelledAt = state.CancelledAt;
    SuspendedAt = state.SuspendedAt;
    CancellationReason = NormalizeOptional(state.CancellationReason);
    CreatedAt = state.CreatedAt;
    UpdatedAt = state.CreatedAt;
  }

  private sealed class BusinessSubscriptionState
  {
    public required Guid Id { get; init; }
    public required BusinessId BusinessId { get; init; }
    public required Guid PlanId { get; init; }
    public required SubscriptionStatus Status { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? TrialEndsAt { get; init; }
    public DateTimeOffset? CurrentPeriodStart { get; init; }
    public DateTimeOffset? CurrentPeriodEnd { get; init; }
    public DateTimeOffset? CancelledAt;
    public DateTimeOffset? SuspendedAt;
    public string? CancellationReason;
    public required DateTimeOffset CreatedAt { get; init; }
  }

  /// <summary>Unique identifier for this subscription</summary>
  public Guid Id { get; private set; }

  /// <summary>The business that owns this subscription</summary>
  public BusinessId BusinessId { get; private set; }

  /// <summary>The plan this subscription is using</summary>
  public Guid PlanId { get; private set; }

  /// <summary>Current state of the subscription (Trial, Active, Expired, etc.)</summary>
  public SubscriptionStatus Status { get; private set; }

  /// <summary>When the subscription started (trial or paid)</summary>
  public DateTimeOffset StartedAt { get; private set; }

  /// <summary>When the trial ends (null if not on trial)</summary>
  public DateTimeOffset? TrialEndsAt { get; private set; }

  /// <summary>When the current billing period started (null if on trial)</summary>
  public DateTimeOffset? CurrentPeriodStart { get; private set; }

  /// <summary>When the current billing period ends (null if on trial)</summary>
  public DateTimeOffset? CurrentPeriodEnd { get; private set; }

  /// <summary>When the subscription was cancelled (null if not cancelled)</summary>
  public DateTimeOffset? CancelledAt { get; private set; }

  /// <summary>When the subscription was suspended (null if not suspended)</summary>
  public DateTimeOffset? SuspendedAt { get; private set; }

  /// <summary>Optional reason captured when the subscription is cancelled</summary>
  public string? CancellationReason { get; private set; }

  /// <summary>When this subscription record was created</summary>
  public DateTimeOffset CreatedAt { get; private set; }

  /// <summary>When this subscription was last modified</summary>
  public DateTimeOffset UpdatedAt { get; private set; }

  /// <summary>Create a trial subscription for a new business</summary>
  public static BusinessSubscription StartTrial(
    Guid id,
    BusinessId businessId,
    Guid planId,
    DateTimeOffset startedAt,
    DateTimeOffset trialEndsAt)
  {
    return new BusinessSubscription(new BusinessSubscriptionState
    {
      Id = id,
      BusinessId = businessId,
      PlanId = planId,
      Status = SubscriptionStatus.Trial,
      StartedAt = startedAt,
      TrialEndsAt = trialEndsAt,
      CancelledAt = null,
      SuspendedAt = null,
      CancellationReason = null,
      CreatedAt = startedAt
    });
  }

  /// <summary>Create an active subscription directly (admin action)</summary>
  public static BusinessSubscription CreateActive(
    Guid id,
    BusinessId businessId,
    Guid planId,
    DateTimeOffset startedAt,
    DateTimeOffset currentPeriodEnd)
  {
    return new BusinessSubscription(new BusinessSubscriptionState
    {
      Id = id,
      BusinessId = businessId,
      PlanId = planId,
      Status = SubscriptionStatus.Active,
      StartedAt = startedAt,
      CurrentPeriodStart = startedAt,
      CurrentPeriodEnd = currentPeriodEnd,
      CancelledAt = null,
      SuspendedAt = null,
      CancellationReason = null,
      CreatedAt = startedAt
    });
  }

  /// <summary>Transition from trial to active (when payment is received)</summary>
  public void ActivateFromTrial(DateTimeOffset activeStart, DateTimeOffset periodEnd, DateTimeOffset now)
  {
    if (Status != SubscriptionStatus.Trial)
    {
      throw new InvalidOperationException($"Cannot activate subscription with status {Status}. Only Trial subscriptions can be activated.");
    }

    Status = SubscriptionStatus.Active;
    StartedAt = activeStart;
    TrialEndsAt = null;
    CurrentPeriodStart = activeStart;
    CurrentPeriodEnd = periodEnd;
    UpdatedAt = now;
  }

  /// <summary>Change the plan the business is subscribed to</summary>
  public void ChangePlan(Guid newPlanId, DateTimeOffset newPeriodEnd, DateTimeOffset now)
  {
    if (Status == SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException("Cannot change plan for a cancelled subscription.");
    }

    if (Status == SubscriptionStatus.Expired)
    {
      throw new InvalidOperationException("Cannot change plan for an expired subscription. Reactivate first.");
    }

    PlanId = newPlanId;
    CurrentPeriodStart = now;
    CurrentPeriodEnd = newPeriodEnd;
    UpdatedAt = now;
  }

  /// <summary>Mark the subscription as past due (payment grace period)</summary>
  public void MarkPastDue(DateTimeOffset now)
  {
    if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
    {
      throw new InvalidOperationException($"Cannot mark {Status} subscription as past due.");
    }

    Status = SubscriptionStatus.PastDue;
    UpdatedAt = now;
  }

  /// <summary>Suspend the subscription (block all operations)</summary>
  public void Suspend(DateTimeOffset now)
  {
    if (Status is SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException("Cannot suspend a cancelled subscription.");
    }

    Status = SubscriptionStatus.Suspended;
    SuspendedAt = now;
    UpdatedAt = now;
  }

  /// <summary>Reactivate a suspended subscription</summary>
  public void Reactivate(DateTimeOffset now)
  {
    if (Status != SubscriptionStatus.Suspended)
    {
      throw new InvalidOperationException($"Cannot reactivate subscription with status {Status}. Only Suspended subscriptions can be reactivated.");
    }

    Status = SubscriptionStatus.Active;
    SuspendedAt = null;
    UpdatedAt = now;
  }

  /// <summary>Reactivate a cancelled subscription with the same plan</summary>
  public void Reactivate(DateTimeOffset newPeriodEnd, DateTimeOffset now)
  {
    if (Status != SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException($"Cannot reactivate subscription with status {Status}. Only Cancelled subscriptions can be reactivated.");
    }

    Status = SubscriptionStatus.Active;
    CurrentPeriodStart = now;
    CurrentPeriodEnd = newPeriodEnd;
    CancelledAt = null;
    CancellationReason = null;
    UpdatedAt = now;
  }

  /// <summary>Mark the subscription as expired (trial/period ended)</summary>
  public void MarkExpired(DateTimeOffset now)
  {
    if (Status == SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException("Cannot mark a cancelled subscription as expired.");
    }

    Status = SubscriptionStatus.Expired;
    UpdatedAt = now;
  }

  /// <summary>Cancel the subscription (final state)</summary>
  public void Cancel(DateTimeOffset now, string? reason = null)
  {
    if (Status == SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException("Subscription is already cancelled.");
    }

    Status = SubscriptionStatus.Cancelled;
    CancelledAt = now;
    CurrentPeriodEnd = null;
    CurrentPeriodStart = null;
    TrialEndsAt = null;
    SuspendedAt = null;
    CancellationReason = NormalizeOptional(reason);
    UpdatedAt = now;
  }

  /// <summary>Restore a cancelled subscription to trial</summary>
  public void ReactivateFromCancelled(
    Guid newPlanId,
    DateTimeOffset startedAt,
    DateTimeOffset trialEndsAt,
    DateTimeOffset now)
  {
    if (Status != SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException($"Cannot reactivate subscription with status {Status}. Only Cancelled subscriptions can be reactivated.");
    }

    PlanId = newPlanId;
    Status = SubscriptionStatus.Trial;
    StartedAt = startedAt;
    TrialEndsAt = trialEndsAt;
    CurrentPeriodStart = null;
    CurrentPeriodEnd = null;
    CancelledAt = null;
    SuspendedAt = null;
    CancellationReason = null;
    UpdatedAt = now;
  }

  /// <summary>Reactivate a cancelled subscription and change the plan</summary>
  public void ReactivateWithPlanChange(Guid newPlanId, DateTimeOffset newPeriodEnd, DateTimeOffset now)
  {
    if (Status != SubscriptionStatus.Cancelled)
    {
      throw new InvalidOperationException($"Cannot reactivate subscription with status {Status}. Only Cancelled subscriptions can be reactivated.");
    }

    PlanId = newPlanId;
    Status = SubscriptionStatus.Active;
    CurrentPeriodStart = now;
    CurrentPeriodEnd = newPeriodEnd;
    CancelledAt = null;
    CancellationReason = null;
    UpdatedAt = now;
  }

  /// <summary>Check if the subscription is expired or should be expired</summary>
  public bool IsExpiredOrShouldExpire(DateTimeOffset now)
  {
    if (Status == SubscriptionStatus.Expired)
    {
      return true;
    }

    if (Status == SubscriptionStatus.Trial && TrialEndsAt.HasValue && now >= TrialEndsAt.Value)
    {
      return true;
    }

    if (Status == SubscriptionStatus.Active && CurrentPeriodEnd.HasValue && now >= CurrentPeriodEnd.Value)
    {
      return true;
    }

    return false;
  }

  /// <summary>Check if the subscription is currently active and valid</summary>
  public bool IsActiveAndValid(DateTimeOffset now)
  {
    if (Status != SubscriptionStatus.Active)
    {
      return false;
    }

    if (CurrentPeriodEnd.HasValue && now >= CurrentPeriodEnd.Value)
    {
      return false;
    }

    return true;
  }

  private static void ValidateStatusDates(
    SubscriptionStatus status,
    DateTimeOffset? trialEndsAt,
    DateTimeOffset? currentPeriodStart,
    DateTimeOffset? currentPeriodEnd,
    DateTimeOffset? cancelledAt)
  {
    switch (status)
    {
      case SubscriptionStatus.Trial:
        if (!trialEndsAt.HasValue)
        {
          throw new InvalidOperationException("Trial subscription must have a TrialEndsAt date.");
        }
        break;

      case SubscriptionStatus.Active:
        if (!currentPeriodStart.HasValue)
        {
          throw new InvalidOperationException("Active subscription must have a CurrentPeriodStart date.");
        }

        if (!currentPeriodEnd.HasValue)
        {
          throw new InvalidOperationException("Active subscription must have a CurrentPeriodEnd date.");
        }
        break;

      case SubscriptionStatus.Cancelled:
        if (!cancelledAt.HasValue)
        {
          throw new InvalidOperationException("Cancelled subscription must have a CancelledAt date.");
        }
        break;
    }
  }

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
