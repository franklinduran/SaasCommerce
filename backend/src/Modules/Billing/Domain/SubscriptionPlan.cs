namespace SaasCommerce.Modules.Billing.Domain;

/// <summary>
/// Represents a subscription plan (Basic, Pro, Premium, etc.)
/// Plans define feature sets and limits that businesses can purchase.
/// Plans are global (not tenant-specific) and managed by admins.
/// </summary>
public sealed class SubscriptionPlan
{
  private SubscriptionPlan()
  {
  }

  private SubscriptionPlan(
    Guid id,
    SubscriptionPlanDefinition definition,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Plan id cannot be empty.", nameof(id));
    }

    ValidateDefinition(definition);

    Id = id;
    ApplyDefinition(definition);
    IsActive = true;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  /// <summary>Unique identifier for the plan</summary>
  public Guid Id { get; private set; }

  /// <summary>Display name of the plan (e.g., "Basic", "Pro", "Premium")</summary>
  public string Name { get; private set; } = string.Empty;

  /// <summary>Stable code used by billing and integrations (e.g., BASIC, PRO, PREMIUM)</summary>
  public string Code { get; private set; } = string.Empty;

  /// <summary>Detailed description of what's included in the plan</summary>
  public string Description { get; private set; } = string.Empty;

  /// <summary>Monthly price in USD</summary>
  public decimal MonthlyPrice { get; private set; }

  /// <summary>Maximum number of branches/locations allowed</summary>
  public int MaxBranches { get; private set; }

  /// <summary>Maximum number of team members allowed</summary>
  public int MaxUsers { get; private set; }

  /// <summary>Maximum number of products in catalog</summary>
  public int MaxProducts { get; private set; }

  /// <summary>Maximum number of sales/transactions allowed per calendar month</summary>
  public int MaxSalesPerMonth { get; private set; }

  /// <summary>Bitmask of enabled features for this plan</summary>
  public SubscriptionFeatures Features { get; private set; }

  /// <summary>Whether this plan is available for new subscriptions</summary>
  public bool IsActive { get; private set; }

  /// <summary>When the plan was created</summary>
  public DateTimeOffset CreatedAt { get; private set; }

  /// <summary>When the plan was last modified</summary>
  public DateTimeOffset UpdatedAt { get; private set; }

  /// <summary>Factory method to create a new subscription plan</summary>
  public static SubscriptionPlan Create(
    Guid id,
    SubscriptionPlanDefinition definition,
    DateTimeOffset createdAt)
  {
    return new SubscriptionPlan(
      id,
      definition,
      createdAt);
  }

  /// <summary>Activate the plan for new subscriptions</summary>
  public void Activate(DateTimeOffset now)
  {
    IsActive = true;
    UpdatedAt = now;
  }

  /// <summary>Deactivate the plan - no new subscriptions allowed, but existing ones continue</summary>
  public void Deactivate(DateTimeOffset now)
  {
    IsActive = false;
    UpdatedAt = now;
  }

  /// <summary>Update plan details (admin operation)</summary>
  public void Update(
    SubscriptionPlanDefinition definition,
    DateTimeOffset now)
  {
    ValidateDefinition(definition);
    ApplyDefinition(definition);
    UpdatedAt = now;
  }

  /// <summary>Check if this plan has a specific feature enabled</summary>
  public bool HasFeature(SubscriptionFeatures feature)
  {
    return (Features & feature) != 0;
  }

  private static string NormalizeCode(string code)
    => code.Trim().ToUpperInvariant();

  private void ApplyDefinition(SubscriptionPlanDefinition definition)
  {
    Name = definition.Name.Trim();
    Code = NormalizeCode(definition.Code);
    Description = definition.Description?.Trim() ?? string.Empty;
    MonthlyPrice = definition.MonthlyPrice;
    MaxBranches = definition.MaxBranches;
    MaxUsers = definition.MaxUsers;
    MaxProducts = definition.MaxProducts;
    MaxSalesPerMonth = definition.MaxSalesPerMonth;
    Features = definition.Features;
  }

  private static void ValidateDefinition(SubscriptionPlanDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);

    if (string.IsNullOrWhiteSpace(definition.Name))
    {
      throw new ArgumentException("Plan name is required.", nameof(definition));
    }

    if (string.IsNullOrWhiteSpace(definition.Code))
    {
      throw new ArgumentException("Plan code is required.", nameof(definition));
    }

    if (definition.MonthlyPrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(definition), "Monthly price cannot be negative.");
    }

    if (definition.MaxBranches < 1
      || definition.MaxUsers < 1
      || definition.MaxProducts < 1
      || definition.MaxSalesPerMonth < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(definition), "Plan limits must be at least 1.");
    }
  }
}

public sealed class SubscriptionPlanDefinition
{
  public required string Name { get; init; }
  public required string Code { get; init; }
  public string? Description { get; init; }
  public required decimal MonthlyPrice { get; init; }
  public required int MaxBranches { get; init; }
  public required int MaxUsers { get; init; }
  public required int MaxProducts { get; init; }
  public required int MaxSalesPerMonth { get; init; }
  public required SubscriptionFeatures Features { get; init; }
}
