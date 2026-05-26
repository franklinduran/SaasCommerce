using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Services;
using SaasCommerce.Modules.Billing.Application.Subscriptions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Plans;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Modules.Tests;

public sealed class BillingSubscriptionCoverageTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public void BusinessSubscription_ShouldStartTrial_WithExpectedState()
  {
    var subscription = TrialSubscription();

    subscription.Status.Should().Be(SubscriptionStatus.Trial);
    subscription.TrialEndsAt.Should().Be(Now.AddDays(14));
    subscription.CurrentPeriodStart.Should().BeNull();
    subscription.CurrentPeriodEnd.Should().BeNull();
    subscription.UpdatedAt.Should().Be(subscription.CreatedAt);
  }

  [Fact]
  public void BusinessSubscription_ShouldCreateActive_WithBillingPeriod()
  {
    var subscription = ActiveSubscription();

    subscription.Status.Should().Be(SubscriptionStatus.Active);
    subscription.CurrentPeriodStart.Should().Be(Now);
    subscription.CurrentPeriodEnd.Should().Be(Now.AddMonths(1));
    subscription.TrialEndsAt.Should().BeNull();
  }

  [Fact]
  public void BusinessSubscription_ShouldRejectEmptyIds()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var planId = Guid.NewGuid();

    FluentActions.Invoking(() => BusinessSubscription.StartTrial(
        Guid.Empty,
        businessId,
        planId,
        Now,
        Now.AddDays(14)))
      .Should().Throw<ArgumentException>();

    FluentActions.Invoking(() => BusinessSubscription.CreateActive(
        Guid.NewGuid(),
        businessId,
        Guid.Empty,
        Now,
        Now.AddMonths(1)))
      .Should().Throw<ArgumentException>();
  }

  [Fact]
  public void BusinessSubscription_ShouldActivateTrial()
  {
    var subscription = TrialSubscription();

    subscription.ActivateFromTrial(Now.AddDays(1), Now.AddMonths(1), Now.AddDays(1));

    subscription.Status.Should().Be(SubscriptionStatus.Active);
    subscription.StartedAt.Should().Be(Now.AddDays(1));
    subscription.TrialEndsAt.Should().BeNull();
    subscription.CurrentPeriodStart.Should().Be(Now.AddDays(1));
    subscription.CurrentPeriodEnd.Should().Be(Now.AddMonths(1));
  }

  [Fact]
  public void BusinessSubscription_ShouldRejectInvalidTrialActivation()
  {
    var subscription = ActiveSubscription();

    FluentActions.Invoking(() => subscription.ActivateFromTrial(Now, Now.AddMonths(1), Now))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*Only Trial subscriptions*");
  }

  [Fact]
  public void BusinessSubscription_ShouldChangePlan_WhenAllowed()
  {
    var subscription = ActiveSubscription();
    var newPlanId = Guid.NewGuid();

    subscription.ChangePlan(newPlanId, Now.AddMonths(2), Now.AddDays(2));

    subscription.PlanId.Should().Be(newPlanId);
    subscription.CurrentPeriodStart.Should().Be(Now.AddDays(2));
    subscription.CurrentPeriodEnd.Should().Be(Now.AddMonths(2));
  }

  [Fact]
  public void BusinessSubscription_ShouldRejectPlanChange_ForTerminalStates()
  {
    var cancelled = ActiveSubscription();
    cancelled.Cancel(Now, "done");

    FluentActions.Invoking(() => cancelled.ChangePlan(Guid.NewGuid(), Now.AddMonths(1), Now))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*cancelled*");

    var expired = ActiveSubscription();
    expired.MarkExpired(Now);

    FluentActions.Invoking(() => expired.ChangePlan(Guid.NewGuid(), Now.AddMonths(1), Now))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*expired*");
  }

  [Fact]
  public void BusinessSubscription_ShouldMoveThroughPastDueSuspendReactivateAndExpire()
  {
    var subscription = ActiveSubscription();

    subscription.MarkPastDue(Now.AddDays(1));
    subscription.Status.Should().Be(SubscriptionStatus.PastDue);

    subscription.Suspend(Now.AddDays(2));
    subscription.Status.Should().Be(SubscriptionStatus.Suspended);
    subscription.SuspendedAt.Should().Be(Now.AddDays(2));

    subscription.Reactivate(Now.AddDays(3));
    subscription.Status.Should().Be(SubscriptionStatus.Active);
    subscription.SuspendedAt.Should().BeNull();

    subscription.MarkExpired(Now.AddDays(4));
    subscription.Status.Should().Be(SubscriptionStatus.Expired);
  }

  [Fact]
  public void BusinessSubscription_ShouldRejectInvalidStatusTransitions()
  {
    var cancelled = ActiveSubscription();
    cancelled.Cancel(Now, null);

    FluentActions.Invoking(() => cancelled.MarkPastDue(Now))
      .Should().Throw<InvalidOperationException>();
    FluentActions.Invoking(() => cancelled.Suspend(Now))
      .Should().Throw<InvalidOperationException>();
    FluentActions.Invoking(() => cancelled.MarkExpired(Now))
      .Should().Throw<InvalidOperationException>();
    FluentActions.Invoking(() => cancelled.Cancel(Now))
      .Should().Throw<InvalidOperationException>();

    var active = ActiveSubscription();
    FluentActions.Invoking(() => active.Reactivate(Now))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*Only Suspended subscriptions*");
    FluentActions.Invoking(() => active.Reactivate(Now.AddMonths(1), Now))
      .Should().Throw<InvalidOperationException>()
      .WithMessage("*Only Cancelled subscriptions*");
  }

  [Fact]
  public void BusinessSubscription_ShouldCancelAndNormalizeReason()
  {
    var subscription = ActiveSubscription();

    subscription.Cancel(Now.AddDays(5), "  customer request  ");

    subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
    subscription.CancelledAt.Should().Be(Now.AddDays(5));
    subscription.CancellationReason.Should().Be("customer request");
    subscription.CurrentPeriodStart.Should().BeNull();
    subscription.CurrentPeriodEnd.Should().BeNull();
    subscription.TrialEndsAt.Should().BeNull();
  }

  [Fact]
  public void BusinessSubscription_ShouldReactivateCancelledSubscription()
  {
    var subscription = ActiveSubscription();
    subscription.Cancel(Now, "old");

    subscription.Reactivate(Now.AddMonths(1), Now.AddDays(1));

    subscription.Status.Should().Be(SubscriptionStatus.Active);
    subscription.CurrentPeriodStart.Should().Be(Now.AddDays(1));
    subscription.CurrentPeriodEnd.Should().Be(Now.AddMonths(1));
    subscription.CancelledAt.Should().BeNull();
    subscription.CancellationReason.Should().BeNull();
  }

  [Fact]
  public void BusinessSubscription_ShouldReactivateCancelledAsTrial()
  {
    var subscription = ActiveSubscription();
    var newPlanId = Guid.NewGuid();
    subscription.Cancel(Now, "old");

    subscription.ReactivateFromCancelled(newPlanId, Now.AddDays(1), Now.AddDays(15), Now.AddDays(1));

    subscription.PlanId.Should().Be(newPlanId);
    subscription.Status.Should().Be(SubscriptionStatus.Trial);
    subscription.TrialEndsAt.Should().Be(Now.AddDays(15));
    subscription.CurrentPeriodStart.Should().BeNull();
  }

  [Fact]
  public void BusinessSubscription_ShouldReactivateCancelledWithPlanChange()
  {
    var subscription = ActiveSubscription();
    var newPlanId = Guid.NewGuid();
    subscription.Cancel(Now, "old");

    subscription.ReactivateWithPlanChange(newPlanId, Now.AddMonths(2), Now.AddDays(1));

    subscription.PlanId.Should().Be(newPlanId);
    subscription.Status.Should().Be(SubscriptionStatus.Active);
    subscription.CurrentPeriodEnd.Should().Be(Now.AddMonths(2));
  }

  [Fact]
  public void BusinessSubscription_ShouldReportExpirationAndValidity()
  {
    var trial = TrialSubscription();
    trial.IsExpiredOrShouldExpire(Now.AddDays(13)).Should().BeFalse();
    trial.IsExpiredOrShouldExpire(Now.AddDays(14)).Should().BeTrue();
    trial.IsActiveAndValid(Now).Should().BeFalse();

    var active = ActiveSubscription();
    active.IsExpiredOrShouldExpire(Now.AddDays(15)).Should().BeFalse();
    active.IsExpiredOrShouldExpire(Now.AddMonths(1)).Should().BeTrue();
    active.IsActiveAndValid(Now.AddDays(15)).Should().BeTrue();
    active.IsActiveAndValid(Now.AddMonths(1)).Should().BeFalse();

    active.MarkExpired(Now);
    active.IsExpiredOrShouldExpire(Now).Should().BeTrue();
  }

  [Fact]
  public void SubscriptionPlan_ShouldValidateAndUpdateDefinition()
  {
    var plan = Plan(code: " basic ");

    plan.Code.Should().Be("BASIC");
    plan.Name.Should().Be("Basic");
    plan.HasFeature(SubscriptionFeatures.Sales).Should().BeTrue();

    plan.Update(
      new SubscriptionPlanDefinition
      {
        Name = " Pro ",
        Code = " pro ",
        Description = "  Better  ",
        MonthlyPrice = 49,
        MaxBranches = 5,
        MaxUsers = 20,
        MaxProducts = 500,
        MaxSalesPerMonth = 10000,
        Features = SubscriptionFeatures.Sales | SubscriptionFeatures.Reports
      },
      Now.AddDays(1));

    plan.Code.Should().Be("PRO");
    plan.Description.Should().Be("Better");
    plan.MonthlyPrice.Should().Be(49);
    plan.UpdatedAt.Should().Be(Now.AddDays(1));
    plan.HasFeature(SubscriptionFeatures.Reports).Should().BeTrue();
  }

  [Fact]
  public void SubscriptionPlan_ShouldRejectInvalidDefinitions()
  {
    FluentActions.Invoking(() => SubscriptionPlan.Create(Guid.Empty, PlanDefinition(), Now))
      .Should().Throw<ArgumentException>();
    FluentActions.Invoking(() => SubscriptionPlan.Create(Guid.NewGuid(), PlanDefinition(name: " "), Now))
      .Should().Throw<ArgumentException>();
    FluentActions.Invoking(() => SubscriptionPlan.Create(Guid.NewGuid(), PlanDefinition(code: " "), Now))
      .Should().Throw<ArgumentException>();
    FluentActions.Invoking(() => SubscriptionPlan.Create(Guid.NewGuid(), PlanDefinition(price: -1), Now))
      .Should().Throw<ArgumentOutOfRangeException>();
    FluentActions.Invoking(() => SubscriptionPlan.Create(Guid.NewGuid(), PlanDefinition(maxBranches: 0), Now))
      .Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public async Task SubscriptionPlanHandlers_ShouldCreateUpdateActivateDeactivateAndFetchPlans()
  {
    var scenario = new BillingScenario();
    var create = new CreateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);

    var created = await create.Handle(new CreateSubscriptionPlanCommand(
      "Pro",
      "pro",
      null,
      39,
      3,
      10,
      250,
      1000,
      true,
      true,
      true));

    created.IsSuccess.Should().BeTrue();
    scenario.Plans.Items.Should().ContainSingle();
    scenario.UnitOfWork.SaveCount.Should().Be(1);

    var planId = scenario.Plans.Items.Single().Id;
    var get = new GetSubscriptionPlanByIdQueryHandler(scenario.Plans);
    var fetched = await get.Handle(new GetSubscriptionPlanByIdQuery(planId));
    fetched.IsSuccess.Should().BeTrue();
    fetched.Value.Code.Should().Be("PRO");

    var update = new UpdateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    var updated = await update.Handle(new UpdateSubscriptionPlanCommand(
      planId,
      "Premium",
      "premium",
      "Full",
      99,
      8,
      40,
      2000,
      5000,
      false,
      true,
      false));

    updated.IsSuccess.Should().BeTrue();
    updated.Value.Name.Should().Be("Premium");
    updated.Value.Features.Should().NotContain(nameof(SubscriptionFeatures.InventoryTransfers));
    updated.Value.Features.Should().Contain(nameof(SubscriptionFeatures.Reports));

    var deactivate = new DeactivateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    (await deactivate.Handle(new DeactivateSubscriptionPlanCommand(planId))).Value.IsActive.Should().BeFalse();

    var activate = new ActivateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    (await activate.Handle(new ActivateSubscriptionPlanCommand(planId))).Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task SubscriptionPlanHandlers_ShouldReturnFailures()
  {
    var scenario = new BillingScenario();
    var existing = Plan(code: "DUP");
    scenario.Plans.Items.Add(existing);

    var create = new CreateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    (await create.Handle(new CreateSubscriptionPlanCommand("Dup", "DUP", null, 10, 1, 1, 1, 1, false, false, false)))
      .Error.Should().Be(SubscriptionErrors.DuplicatePlanCode);

    (await create.Handle(new CreateSubscriptionPlanCommand("", "bad", null, 10, 1, 1, 1, 1, false, false, false)))
      .Error.Should().Be(SubscriptionErrors.InvalidPlanData);

    var get = new GetSubscriptionPlanByIdQueryHandler(scenario.Plans);
    (await get.Handle(new GetSubscriptionPlanByIdQuery(Guid.NewGuid())))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);

    var update = new UpdateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    (await update.Handle(new UpdateSubscriptionPlanCommand(Guid.NewGuid(), "Missing", "MISSING", null, 1, 1, 1, 1, 1, false, false, false)))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);

    var duplicate = Plan(code: "OTHER");
    scenario.Plans.Items.Add(duplicate);
    (await update.Handle(new UpdateSubscriptionPlanCommand(existing.Id, "Other", "OTHER", null, 1, 1, 1, 1, 1, false, false, false)))
      .Error.Should().Be(SubscriptionErrors.DuplicatePlanCode);

    var activate = new ActivateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    (await activate.Handle(new ActivateSubscriptionPlanCommand(Guid.NewGuid())))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);

    var deactivate = new DeactivateSubscriptionPlanCommandHandler(scenario.Plans, scenario.Clock, scenario.UnitOfWork);
    (await deactivate.Handle(new DeactivateSubscriptionPlanCommand(Guid.NewGuid())))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);
  }

  [Theory]
  [InlineData("branch", 1, 2, true, "OK")]
  [InlineData("branch", 2, 2, false, "SUBSCRIPTION_BRANCH_LIMIT_REACHED")]
  [InlineData("user", 4, 5, true, "OK")]
  [InlineData("user", 5, 5, false, "SUBSCRIPTION_USER_LIMIT_REACHED")]
  [InlineData("product", 9, 10, true, "OK")]
  [InlineData("product", 10, 10, false, "SUBSCRIPTION_PRODUCT_LIMIT_REACHED")]
  [InlineData("sale", 99, 100, true, "OK")]
  [InlineData("sale", 100, 100, false, "SUBSCRIPTION_SALES_LIMIT_REACHED")]
  public async Task SubscriptionLimitChecker_ShouldEvaluateResourceLimits(
    string resource,
    int current,
    int limit,
    bool expectedAllowed,
    string expectedCode)
  {
    var scenario = BillingScenario.WithSubscription();
    scenario.Usage.Branches = resource == "branch" ? current : 0;
    scenario.Usage.Users = resource == "user" ? current : 0;
    scenario.Usage.Products = resource == "product" ? current : 0;
    scenario.Usage.MonthlySales = resource == "sale" ? current : 0;
    scenario.Plan = Plan(maxBranches: limit, maxUsers: limit, maxProducts: limit, maxSales: limit);
    scenario.Plans.Items[0] = scenario.Plan;
    scenario.Subscriptions.Current = ActiveSubscription(scenario.Plan.Id);
    var checker = scenario.CreateLimitChecker();

    var result = resource switch
    {
      "branch" => await checker.CanCreateBranchAsync(scenario.BusinessId),
      "user" => await checker.CanCreateUserAsync(scenario.BusinessId),
      "product" => await checker.CanCreateProductAsync(scenario.BusinessId),
      _ => await checker.CanCreateSaleAsync(scenario.BusinessId)
    };

    result.IsAllowed.Should().Be(expectedAllowed);
    result.Code.Should().Be(expectedCode);
    result.CurrentUsage.Should().Be(current);
    result.MaxAllowed.Should().Be(limit);
  }

  [Fact]
  public async Task SubscriptionLimitChecker_ShouldReturnNoSubscription_WhenMissingSubscriptionOrPlan()
  {
    var scenario = new BillingScenario();
    var checker = scenario.CreateLimitChecker();

    var noSubscription = await checker.CanCreateBranchAsync(scenario.BusinessId);
    noSubscription.IsAllowed.Should().BeFalse();
    noSubscription.Code.Should().Be("NO_SUBSCRIPTION");

    scenario.Subscriptions.Current = ActiveSubscription(planId: Guid.NewGuid());
    var noPlan = await checker.CanCreateUserAsync(scenario.BusinessId);
    noPlan.IsAllowed.Should().BeFalse();
    noPlan.Code.Should().Be("NO_SUBSCRIPTION");
  }

  [Fact]
  public async Task SubscriptionLimitChecker_ShouldPropagateAccessPolicyFailures()
  {
    var scenario = BillingScenario.WithSubscription();
    scenario.Access.Error = new DomainError("subscription.blocked", "Blocked");
    var checker = scenario.CreateLimitChecker();

    var result = await checker.CanCreateProductAsync(scenario.BusinessId);

    result.IsAllowed.Should().BeFalse();
    result.Code.Should().Be("subscription.blocked");
    result.Message.Should().Be("Blocked");
  }

  [Fact]
  public async Task SubscriptionLimitChecker_ShouldAllowFeatureSpecificChecks()
  {
    var scenario = BillingScenario.WithSubscription();
    var checker = scenario.CreateLimitChecker();

    (await checker.CanUseInventoryTransfersAsync(scenario.BusinessId)).IsAllowed.Should().BeTrue();
    (await checker.CanUseAdvancedReportsAsync(scenario.BusinessId)).IsAllowed.Should().BeTrue();
    (await checker.CanUseAuditLogsAsync(scenario.BusinessId)).IsAllowed.Should().BeTrue();

    scenario.Access.Error = SubscriptionErrors.FeatureNotAvailable;
    (await checker.CanUseAuditLogsAsync(scenario.BusinessId)).Code.Should().Be(SubscriptionErrors.FeatureNotAvailable.Code);
  }

  [Fact]
  public async Task SubscriptionAccessPolicy_ShouldValidateSubscriptionPlanAndFeature()
  {
    var scenario = BillingScenario.WithSubscription();
    var policy = new SubscriptionAccessPolicy(scenario.Subscriptions, scenario.Plans, scenario.Clock);

    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales)).IsSuccess.Should().BeTrue();
    (await policy.IsSubscriptionActiveAsync(scenario.BusinessId)).Should().BeTrue();

    scenario.Plans.Items.Clear();
    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);

    var limitedPlan = SubscriptionPlan.Create(
      Guid.NewGuid(),
      new SubscriptionPlanDefinition
      {
        Name = "Limited",
        Code = "LIMITED",
        Description = "Limited",
        MonthlyPrice = 1,
        MaxBranches = 1,
        MaxUsers = 1,
        MaxProducts = 1,
        MaxSalesPerMonth = 1,
        Features = SubscriptionFeatures.Sales
      },
      Now);
    scenario.Plans.Items.Add(limitedPlan);
    scenario.Subscriptions.Current = ActiveSubscription(limitedPlan.Id);

    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Reports))
      .Error.Should().Be(SubscriptionErrors.FeatureNotAvailable);

    scenario.Subscriptions.Current = null;
    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales))
      .Error.Should().Be(SubscriptionErrors.SubscriptionNotFound);
    (await policy.GetCurrentSubscriptionStatusAsync(scenario.BusinessId)).Should().BeNull();
  }

  [Fact]
  public async Task SubscriptionAccessPolicy_ShouldReturnFailuresForInactiveStatusesAndExpireStaleSubscriptions()
  {
    var scenario = BillingScenario.WithSubscription();
    var policy = new SubscriptionAccessPolicy(scenario.Subscriptions, scenario.Plans, scenario.Clock);

    var expired = ActiveSubscription(scenario.Plan.Id);
    expired.MarkExpired(Now);
    scenario.Subscriptions.Current = expired;
    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales))
      .Error.Should().Be(SubscriptionErrors.SubscriptionExpired);

    var suspended = ActiveSubscription(scenario.Plan.Id);
    suspended.Suspend(Now);
    scenario.Subscriptions.Current = suspended;
    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales))
      .Error.Should().Be(SubscriptionErrors.SubscriptionSuspended);

    var cancelled = ActiveSubscription(scenario.Plan.Id);
    cancelled.Cancel(Now);
    scenario.Subscriptions.Current = cancelled;
    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales))
      .Error.Should().Be(SubscriptionErrors.SubscriptionCancelled);

    var pastDue = ActiveSubscription(scenario.Plan.Id);
    pastDue.MarkPastDue(Now);
    scenario.Subscriptions.Current = pastDue;
    (await policy.EnsureCanUseFeatureAsync(scenario.BusinessId, SubscriptionFeatures.Sales))
      .Error.Should().Be(SubscriptionErrors.SubscriptionSuspended);

    scenario.Subscriptions.Current = ActiveSubscription(scenario.Plan.Id);
    (await policy.GetCurrentSubscriptionStatusAsync(scenario.BusinessId)).Should().Be(SubscriptionStatus.Active);
  }

  [Fact]
  public async Task GetSubscriptionUsageQueryHandler_ShouldBuildUsageResponseAndFailures()
  {
    var scenario = BillingScenario.WithSubscription();
    scenario.Usage.Branches = scenario.Plan.MaxBranches;
    scenario.Usage.Users = 2;
    scenario.Usage.Products = scenario.Plan.MaxProducts;
    scenario.Usage.MonthlySales = 10;

    var handler = new GetSubscriptionUsageQueryHandler(
      scenario.Subscriptions,
      scenario.Plans,
      scenario.Usage,
      TestCurrentUser.Create(scenario.BusinessId.Value),
      scenario.Clock);

    var result = await handler.Handle(new GetSubscriptionUsageQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.SubscriptionId.Should().Be(scenario.Subscriptions.Current!.Id);
    result.Value.PlanName.Should().Be(scenario.Plan.Name);
    result.Value.Branches.IsAtLimit.Should().BeTrue();
    result.Value.Products.IsAtLimit.Should().BeTrue();
    result.Value.Users.IsAtLimit.Should().BeFalse();
    result.Value.Features.Should().Contain(feature => feature.Name == "Auditoria" && feature.IsEnabled);
    scenario.Usage.LastMonthStart.Should().Be(new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));
    scenario.Usage.LastMonthEnd.Should().Be(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));

    var noContext = await new GetSubscriptionUsageQueryHandler(
        scenario.Subscriptions,
        scenario.Plans,
        scenario.Usage,
        TestCurrentUser.Create(null),
        scenario.Clock)
      .Handle(new GetSubscriptionUsageQuery());
    noContext.Error.Should().Be(SubscriptionErrors.UserContextRequired);

    scenario.Subscriptions.Current = null;
    (await handler.Handle(new GetSubscriptionUsageQuery()))
      .Error.Should().Be(SubscriptionErrors.SubscriptionNotFound);

    scenario.Subscriptions.Current = ActiveSubscription(Guid.NewGuid());
    (await handler.Handle(new GetSubscriptionUsageQuery()))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);
  }

  [Fact]
  public async Task SubscriptionCommandHandlers_ShouldStartChangeCancelAndReactivate()
  {
    var scenario = new BillingScenario();
    var currentUser = TestCurrentUser.Create(scenario.BusinessId.Value);
    var outbox = new RecordingOutboxWriter();
    scenario.Plans.Items.Add(scenario.Plan);

    var trial = await new StartTrialSubscriptionCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new StartTrialSubscriptionCommand(null));

    trial.IsSuccess.Should().BeTrue();
    trial.Value.Status.Should().Be(nameof(SubscriptionStatus.Trial));
    outbox.Events.OfType<BusinessSubscriptionChangedEventV1>().Should().ContainSingle();

    scenario.Subscriptions.Current = ActiveSubscription(scenario.Plan.Id);
    var newPlan = Plan(code: "PRO");
    scenario.Plans.Items.Add(newPlan);

    var changed = await new ChangeBusinessPlanCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new ChangeBusinessPlanCommand(null, newPlan.Id));

    changed.IsSuccess.Should().BeTrue();
    changed.Value.Plan.Id.Should().Be(newPlan.Id);

    var cancelled = await new CancelBusinessSubscriptionCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new CancelBusinessSubscriptionCommand(null));

    cancelled.IsSuccess.Should().BeTrue();
    cancelled.Value.Status.Should().Be(nameof(SubscriptionStatus.Cancelled));

    var reactivated = await new ReactivateBusinessSubscriptionCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new ReactivateBusinessSubscriptionCommand(scenario.Plan.Id));

    reactivated.IsSuccess.Should().BeTrue();
    reactivated.Value.Status.Should().Be(nameof(SubscriptionStatus.Active));
    scenario.UnitOfWork.SaveCount.Should().BeGreaterThan(3);
  }

  [Fact]
  public async Task SubscriptionCommandHandlers_ShouldReturnExpectedFailures()
  {
    var scenario = new BillingScenario();
    var noBusinessUser = TestCurrentUser.Create(null);
    var currentUser = TestCurrentUser.Create(scenario.BusinessId.Value);
    var outbox = new RecordingOutboxWriter();

    var start = new StartTrialSubscriptionCommandHandler(
      scenario.Subscriptions,
      scenario.Plans,
      noBusinessUser,
      scenario.Clock,
      scenario.UnitOfWork,
      outbox);
    (await start.Handle(new StartTrialSubscriptionCommand(null)))
      .Error.Should().Be(SubscriptionErrors.UserContextRequired);

    scenario.Plans.Items.Add(scenario.Plan);
    scenario.Subscriptions.Current = ActiveSubscription(scenario.Plan.Id);
    (await new StartTrialSubscriptionCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new StartTrialSubscriptionCommand(null)))
      .Error.Should().Be(SubscriptionErrors.DuplicateSubscription);

    (await new ChangeBusinessPlanCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new ChangeBusinessPlanCommand(null, Guid.Empty)))
      .Error.Should().Be(SubscriptionErrors.InvalidPlanData);

    (await new ChangeBusinessPlanCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new ChangeBusinessPlanCommand(null, Guid.NewGuid())))
      .Error.Should().Be(SubscriptionErrors.PlanNotFound);

    scenario.Subscriptions.Current = null;
    (await new CancelBusinessSubscriptionCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new CancelBusinessSubscriptionCommand(null)))
      .Error.Should().Be(SubscriptionErrors.SubscriptionNotFound);

    scenario.Subscriptions.Current = ActiveSubscription(scenario.Plan.Id);
    (await new ReactivateBusinessSubscriptionCommandHandler(
        scenario.Subscriptions,
        scenario.Plans,
        currentUser,
        scenario.Clock,
        scenario.UnitOfWork,
        outbox)
      .Handle(new ReactivateBusinessSubscriptionCommand()))
      .Error.Code.Should().Be("subscription.not_cancelled");
  }

  private static BusinessSubscription TrialSubscription(Guid? planId = null)
    => BusinessSubscription.StartTrial(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      planId ?? Guid.NewGuid(),
      Now,
      Now.AddDays(14));

  private static BusinessSubscription ActiveSubscription(Guid? planId = null)
    => BusinessSubscription.CreateActive(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      planId ?? Guid.NewGuid(),
      Now,
      Now.AddMonths(1));

  private static SubscriptionPlan Plan(
    string code = "BASIC",
    int maxBranches = 2,
    int maxUsers = 5,
    int maxProducts = 10,
    int maxSales = 100)
    => SubscriptionPlan.Create(
      Guid.NewGuid(),
      PlanDefinition(code: code, maxBranches: maxBranches, maxUsers: maxUsers, maxProducts: maxProducts, maxSales: maxSales),
      Now);

  private static SubscriptionPlanDefinition PlanDefinition(
    string name = "Basic",
    string code = "BASIC",
    decimal price = 19,
    int maxBranches = 2,
    int maxUsers = 5,
    int maxProducts = 10,
    int maxSales = 100)
    => new()
    {
      Name = name,
      Code = code,
      Description = "Starter",
      MonthlyPrice = price,
      MaxBranches = maxBranches,
      MaxUsers = maxUsers,
      MaxProducts = maxProducts,
      MaxSalesPerMonth = maxSales,
      Features = SubscriptionFeatures.Sales |
        SubscriptionFeatures.Products |
        SubscriptionFeatures.Branches |
        SubscriptionFeatures.Users |
        SubscriptionFeatures.InventoryTransfers |
        SubscriptionFeatures.Reports |
        SubscriptionFeatures.AuditLogs
    };

  private sealed class BillingScenario
  {
    public BusinessId BusinessId { get; } = new(Guid.NewGuid());

    public TestSubscriptionPlanRepository Plans { get; } = new();

    public TestBusinessSubscriptionRepository Subscriptions { get; } = new();

    public TestSubscriptionUsageReader Usage { get; } = new();

    public TestAccessPolicy Access { get; } = new();

    public TestClock Clock { get; } = new();

    public TestUnitOfWork UnitOfWork { get; } = new();

    public SubscriptionPlan Plan { get; set; } = BillingSubscriptionCoverageTests.Plan();

    public static BillingScenario WithSubscription()
    {
      var scenario = new BillingScenario();
      scenario.Subscriptions.Current = ActiveSubscription(scenario.Plan.Id);
      scenario.Plans.Items.Add(scenario.Plan);
      return scenario;
    }

    public SubscriptionLimitChecker CreateLimitChecker()
      => new(Subscriptions, Plans, Usage, Clock, Access);
  }

  private sealed class TestSubscriptionPlanRepository : ISubscriptionPlanRepository
  {
    public List<SubscriptionPlan> Items { get; } = [];

    public Task AddAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
    {
      Items.Add(plan);
      return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyList<SubscriptionPlan>>(Items.Where(plan => plan.IsActive).ToList());

    public Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyList<SubscriptionPlan>>(Items);

    public Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(plan => string.Equals(plan.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task<SubscriptionPlan?> GetByIdAsync(Guid planId, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(plan => plan.Id == planId));

    public Task UpdateAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class TestBusinessSubscriptionRepository : IBusinessSubscriptionRepository
  {
    public BusinessSubscription? Current { get; set; }

    public Task AddAsync(BusinessSubscription subscription, CancellationToken cancellationToken = default)
    {
      Current = subscription;
      return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Current?.BusinessId == businessId);

    public Task<BusinessSubscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
      => Task.FromResult(Current?.Id == subscriptionId ? Current : null);

    public Task<BusinessSubscription?> GetCurrentByBusinessIdAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Current);

    public Task<IReadOnlyList<BusinessSubscription>> GetByStatusAsync(SubscriptionStatus status, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyList<BusinessSubscription>>(Current?.Status == status ? [Current] : []);

    public Task<IReadOnlyList<BusinessSubscription>> GetExpiringAsync(DateTimeOffset beforeDate, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyList<BusinessSubscription>>(Current is not null && Current.IsExpiredOrShouldExpire(beforeDate) ? [Current] : []);

    public Task UpdateAsync(BusinessSubscription subscription, CancellationToken cancellationToken = default)
    {
      Current = subscription;
      return Task.CompletedTask;
    }
  }

  private sealed class TestSubscriptionUsageReader : ISubscriptionUsageReader
  {
    public int Branches { get; set; }
    public int Users { get; set; }
    public int Products { get; set; }
    public int MonthlySales { get; set; }
    public DateTimeOffset? LastMonthStart { get; private set; }
    public DateTimeOffset? LastMonthEnd { get; private set; }

    public Task<int> CountActiveBranchesAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Branches);

    public Task<int> CountActiveUsersAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Users);

    public Task<int> CountActiveProductsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Products);

    public Task<int> CountMonthlySalesAsync(
      BusinessId businessId,
      DateTimeOffset monthStart,
      DateTimeOffset monthEnd,
      CancellationToken cancellationToken = default)
    {
      LastMonthStart = monthStart;
      LastMonthEnd = monthEnd;
      return Task.FromResult(MonthlySales);
    }
  }

  private sealed class TestAccessPolicy : ISubscriptionAccessPolicy
  {
    public DomainError? Error { get; set; }

    public Task<Result> EnsureCanUseFeatureAsync(
      BusinessId businessId,
      SubscriptionFeatures feature,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Error is null ? Result.Success() : Result.Failure(Error));

    public Task<SubscriptionStatus?> GetCurrentSubscriptionStatusAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult<SubscriptionStatus?>(SubscriptionStatus.Active);

    public Task<bool> IsSubscriptionActiveAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(true);
  }

  private sealed class TestClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class TestUnitOfWork : IUnitOfWork
  {
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveCount++;
      return Task.FromResult(1);
    }
  }

  private sealed record TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }

    public Guid? BusinessId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; }

    public static TestCurrentUser Create(Guid? businessId)
      => new()
      {
        UserId = Guid.NewGuid(),
        BusinessId = businessId,
        BranchId = Guid.NewGuid(),
        Roles = ["Admin"],
        IsAuthenticated = true
      };
  }

  private sealed class RecordingOutboxWriter : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(
      TEvent integrationEvent,
      CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }
}

#pragma warning restore CA1707
