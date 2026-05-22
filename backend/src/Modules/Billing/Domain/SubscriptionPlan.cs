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
    string name,
    string code,
    string description,
    decimal monthlyPrice,
    int maxBranches,
    int maxUsers,
    int maxProducts,
    int maxSalesPerMonth,
    SubscriptionFeature features,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Plan id cannot be empty.", nameof(id));
    }

    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Plan name is required.", nameof(name));
    }

    if (string.IsNullOrWhiteSpace(code))
    {
      throw new ArgumentException("Plan code is required.", nameof(code));
    }

    if (monthlyPrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(monthlyPrice), "Monthly price cannot be negative.");
    }

    if (maxBranches < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxBranches), "Max branches must be at least 1.");
    }

    if (maxUsers < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxUsers), "Max users must be at least 1.");
    }

    if (maxProducts < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxProducts), "Max products must be at least 1.");
    }

    if (maxSalesPerMonth < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxSalesPerMonth), "Max sales per month must be at least 1.");
    }

    Id = id;
    Name = name.Trim();
    Code = NormalizeCode(code);
    Description = description ?? string.Empty;
    MonthlyPrice = monthlyPrice;
    MaxBranches = maxBranches;
    MaxUsers = maxUsers;
    MaxProducts = maxProducts;
    MaxSalesPerMonth = maxSalesPerMonth;
    Features = features;
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
  public SubscriptionFeature Features { get; private set; }

  /// <summary>Whether this plan is available for new subscriptions</summary>
  public bool IsActive { get; private set; }

  /// <summary>When the plan was created</summary>
  public DateTimeOffset CreatedAt { get; private set; }

  /// <summary>When the plan was last modified</summary>
  public DateTimeOffset UpdatedAt { get; private set; }

  /// <summary>Factory method to create a new subscription plan</summary>
  public static SubscriptionPlan Create(
    Guid id,
    string name,
    string code,
    string description,
    decimal monthlyPrice,
    int maxBranches,
    int maxUsers,
    int maxProducts,
    int maxSalesPerMonth,
    SubscriptionFeature features,
    DateTimeOffset createdAt)
  {
    return new SubscriptionPlan(
      id,
      name,
      code,
      description,
      monthlyPrice,
      maxBranches,
      maxUsers,
      maxProducts,
      maxSalesPerMonth,
      features,
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
    string name,
    string code,
    string description,
    decimal monthlyPrice,
    int maxBranches,
    int maxUsers,
    int maxProducts,
    int maxSalesPerMonth,
    SubscriptionFeature features,
    DateTimeOffset now)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Plan name is required.", nameof(name));
    }

    if (string.IsNullOrWhiteSpace(code))
    {
      throw new ArgumentException("Plan code is required.", nameof(code));
    }

    if (monthlyPrice < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(monthlyPrice), "Monthly price cannot be negative.");
    }

    if (maxBranches < 1 || maxUsers < 1 || maxProducts < 1 || maxSalesPerMonth < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxBranches), "Plan limits must be at least 1.");
    }

    Name = name.Trim();
    Code = NormalizeCode(code);
    Description = description?.Trim() ?? string.Empty;
    MonthlyPrice = monthlyPrice;
    MaxBranches = maxBranches;
    MaxUsers = maxUsers;
    MaxProducts = maxProducts;
    MaxSalesPerMonth = maxSalesPerMonth;
    Features = features;
    UpdatedAt = now;
  }

  /// <summary>Check if this plan has a specific feature enabled</summary>
  public bool HasFeature(SubscriptionFeature feature)
  {
    return (Features & feature) != 0;
  }

  private static string NormalizeCode(string code)
    => code.Trim().ToUpperInvariant();
}
