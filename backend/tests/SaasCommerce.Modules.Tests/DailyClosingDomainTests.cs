#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Sales.Application.DailyClosings;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class DailyClosingDomainTests
{
  private static readonly BusinessId TestBusinessId = new(Guid.Parse("11111111-1111-1111-1111-111111111111"));
  private static readonly BranchId TestBranchId = new(Guid.Parse("22222222-2222-2222-2222-222222222222"));
  private static readonly Guid TestUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
  private static readonly DateOnly TestDate = new(2026, 5, 23);
  private static readonly DateTimeOffset Now = new(2026, 5, 23, 18, 0, 0, TimeSpan.Zero);

  // ── DailyClosingCalculator ───────────────────────────────────────────────

  [Fact]
  public void CashDifference_ShouldReturnCountedMinusExpected()
  {
    var result = DailyClosingCalculator.CashDifference(5_000m, 4_800m);
    result.Should().Be(200m);
  }

  [Fact]
  public void CashDifference_ShouldReturnNegative_WhenShortage()
  {
    var result = DailyClosingCalculator.CashDifference(4_500m, 4_800m);
    result.Should().Be(-300m);
  }

  [Fact]
  public void ExpenseRatio_ShouldReturnCorrectPercentage()
  {
    var result = DailyClosingCalculator.ExpenseRatio(10_000m, 2_500m);
    result.Should().Be(25m);
  }

  [Fact]
  public void ExpenseRatio_ShouldReturnZero_WhenNoSales()
  {
    var result = DailyClosingCalculator.ExpenseRatio(0m, 1_000m);
    result.Should().Be(0m);
  }

  [Fact]
  public void CreditSalesRatio_ShouldReturnCorrectPercentage()
  {
    var result = DailyClosingCalculator.CreditSalesRatio(10_000m, 4_200m);
    result.Should().Be(42m);
  }

  [Fact]
  public void IsHighExpenseRatio_ShouldReturnTrue_WhenAbove30Percent()
  {
    DailyClosingCalculator.IsHighExpenseRatio(35m).Should().BeTrue();
  }

  [Fact]
  public void IsHighExpenseRatio_ShouldReturnFalse_WhenAt30Percent()
  {
    DailyClosingCalculator.IsHighExpenseRatio(30m).Should().BeFalse();
  }

  [Fact]
  public void IsCreditSalesHigh_ShouldReturnTrue_WhenAbove40Percent()
  {
    DailyClosingCalculator.IsCreditSalesHigh(45m).Should().BeTrue();
  }

  [Fact]
  public void ComputeCashExpected_ShouldBeSumOfOpeningAndCashSales()
  {
    var result = DailyClosingCalculator.ComputeCashExpected(1_000m, 3_500m);
    result.Should().Be(4_500m);
  }

  // ── DailyClosing entity ──────────────────────────────────────────────────

  [Fact]
  public void Create_ShouldStartInDraftStatus()
  {
    var closing = BuildClosing();
    closing.Status.Should().Be(DailyClosingStatus.Draft);
  }

  [Fact]
  public void Create_ShouldHaveNullCashCounted_WhenDraft()
  {
    var closing = BuildClosing();
    closing.CashCounted.Should().BeNull();
    closing.CashDifference.Should().BeNull();
  }

  [Fact]
  public void Close_ShouldTransitionToClosedStatus()
  {
    var closing = BuildClosing();
    closing.Close(TestUserId, 5_000m, "Notas de cierre", Now);
    closing.Status.Should().Be(DailyClosingStatus.Closed);
  }

  [Fact]
  public void Close_ShouldRecordCashDifference()
  {
    var closing = BuildClosing(cashExpected: 4_800m);
    closing.Close(TestUserId, 5_000m, null, Now);
    closing.CashDifference.Should().Be(200m);
  }

  [Fact]
  public void Close_ShouldThrow_WhenAlreadyClosed()
  {
    var closing = BuildClosing();
    closing.Close(TestUserId, 5_000m, null, Now);

    Action act = () => closing.Close(TestUserId, 5_000m, null, Now.AddHours(1));
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void AddAlert_ShouldAddAlertToDraftClosing()
  {
    var closing = BuildClosing();
    var alert = DailyClosingAlert.Create(Guid.NewGuid(), closing.Id, DailyClosingAlertType.OpenCashSession, "Alerta de prueba");

    closing.AddAlert(alert);

    closing.Alerts.Should().HaveCount(1);
    closing.Alerts.First().AlertType.Should().Be(DailyClosingAlertType.OpenCashSession);
  }

  [Fact]
  public void AddAlert_ShouldThrow_WhenClosingIsAlreadyClosed()
  {
    var closing = BuildClosing();
    closing.Close(TestUserId, 5_000m, null, Now);
    var alert = DailyClosingAlert.Create(Guid.NewGuid(), closing.Id, DailyClosingAlertType.MissingProductCost, "Test");

    Action act = () => closing.AddAlert(alert);
    act.Should().Throw<InvalidOperationException>();
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static DailyClosing BuildClosing(decimal cashExpected = 4_800m)
    => DailyClosing.Create(
      Guid.NewGuid(),
      TestBusinessId,
      TestBranchId,
      TestUserId,
      TestDate,
      totalSales: 12_000m,
      cashSales: 4_000m,
      transferSales: 5_000m,
      cardSales: 2_000m,
      creditSales: 1_000m,
      salesCount: 30,
      cashExpected: cashExpected,
      totalExpenses: 2_000m,
      totalCost: 7_200m,
      grossProfit: 4_800m,
      estimatedNetProfit: 2_800m,
      grossMarginPercent: 40m,
      netMarginPercent: 23.33m,
      newCreditsAmount: 1_000m,
      newCreditsCount: 2,
      creditPaymentsReceived: 500m,
      notes: null,
      createdAt: Now);
}
