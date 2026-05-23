#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Sales.Application.Profitability;

namespace SaasCommerce.Modules.Tests;

public sealed class ProfitabilityDomainTests
{
  // ── GrossProfit ──────────────────────────────────────────────────────────

  [Fact]
  public void GrossProfit_ShouldReturnSalesMinusCost()
  {
    var result = ProfitabilityCalculator.GrossProfit(1_000m, 600m);
    result.Should().Be(400m);
  }

  [Fact]
  public void GrossProfit_ShouldReturnNegative_WhenCostExceedsSales()
  {
    var result = ProfitabilityCalculator.GrossProfit(500m, 700m);
    result.Should().Be(-200m);
  }

  [Fact]
  public void GrossProfit_ShouldReturnZero_WhenSalesEqualCost()
  {
    var result = ProfitabilityCalculator.GrossProfit(300m, 300m);
    result.Should().Be(0m);
  }

  // ── EstimatedNetProfit ───────────────────────────────────────────────────

  [Fact]
  public void EstimatedNetProfit_ShouldReturnGrossProfitMinusExpenses()
  {
    var result = ProfitabilityCalculator.EstimatedNetProfit(400m, 150m);
    result.Should().Be(250m);
  }

  [Fact]
  public void EstimatedNetProfit_ShouldReturnNegative_WhenExpensesExceedGrossProfit()
  {
    var result = ProfitabilityCalculator.EstimatedNetProfit(100m, 300m);
    result.Should().Be(-200m);
  }

  // ── GrossMarginPercent ───────────────────────────────────────────────────

  [Fact]
  public void GrossMarginPercent_ShouldReturnCorrectPercent()
  {
    // 400 gross profit on 1000 sales = 40%
    var result = ProfitabilityCalculator.GrossMarginPercent(1_000m, 400m);
    result.Should().Be(40m);
  }

  [Fact]
  public void GrossMarginPercent_ShouldReturnZero_WhenSalesAreZero()
  {
    var result = ProfitabilityCalculator.GrossMarginPercent(0m, 0m);
    result.Should().Be(0m);
  }

  [Fact]
  public void GrossMarginPercent_ShouldReturnNegative_WhenGrossProfitIsNegative()
  {
    // -200 gross profit on 1000 sales = -20%
    var result = ProfitabilityCalculator.GrossMarginPercent(1_000m, -200m);
    result.Should().Be(-20m);
  }

  [Fact]
  public void GrossMarginPercent_ShouldRoundToTwoDecimals()
  {
    // 1/3 = 33.33%
    var result = ProfitabilityCalculator.GrossMarginPercent(300m, 100m);
    result.Should().Be(33.33m);
  }

  // ── NetMarginPercent ─────────────────────────────────────────────────────

  [Fact]
  public void NetMarginPercent_ShouldReturnCorrectPercent()
  {
    // 250 net profit on 1000 sales = 25%
    var result = ProfitabilityCalculator.NetMarginPercent(1_000m, 250m);
    result.Should().Be(25m);
  }

  [Fact]
  public void NetMarginPercent_ShouldReturnZero_WhenSalesAreZero()
  {
    var result = ProfitabilityCalculator.NetMarginPercent(0m, 0m);
    result.Should().Be(0m);
  }

  // ── ProductMarginPercent ─────────────────────────────────────────────────

  [Fact]
  public void ProductMarginPercent_ShouldReturnCorrectPercent()
  {
    // Cost 600, Sales 1000 → (1000-600)/1000 = 40%
    var result = ProfitabilityCalculator.ProductMarginPercent(1_000m, 600m);
    result.Should().Be(40m);
  }

  [Fact]
  public void ProductMarginPercent_ShouldReturnNegative_WhenCostExceedsSales()
  {
    // Cost 700, Sales 500 → (500-700)/500 = -40%
    var result = ProfitabilityCalculator.ProductMarginPercent(500m, 700m);
    result.Should().Be(-40m);
  }

  [Fact]
  public void ProductMarginPercent_ShouldReturnZero_WhenSalesAreZero()
  {
    var result = ProfitabilityCalculator.ProductMarginPercent(0m, 0m);
    result.Should().Be(0m);
  }

  [Fact]
  public void ProductMarginPercent_ShouldReturnHundred_WhenCostIsZero()
  {
    var result = ProfitabilityCalculator.ProductMarginPercent(500m, 0m);
    result.Should().Be(100m);
  }

  // ── IsNegativeMargin ─────────────────────────────────────────────────────

  [Fact]
  public void IsNegativeMargin_ShouldReturnTrue_WhenCostExceedsSales()
  {
    ProfitabilityCalculator.IsNegativeMargin(500m, 600m).Should().BeTrue();
  }

  [Fact]
  public void IsNegativeMargin_ShouldReturnFalse_WhenCostEqualsSales()
  {
    ProfitabilityCalculator.IsNegativeMargin(500m, 500m).Should().BeFalse();
  }

  [Fact]
  public void IsNegativeMargin_ShouldReturnFalse_WhenSalesAreZero()
  {
    ProfitabilityCalculator.IsNegativeMargin(0m, 0m).Should().BeFalse();
  }

  [Fact]
  public void IsNegativeMargin_ShouldReturnFalse_WhenCostIsLower()
  {
    ProfitabilityCalculator.IsNegativeMargin(1_000m, 600m).Should().BeFalse();
  }

  // ── IsLowMargin ──────────────────────────────────────────────────────────

  [Fact]
  public void IsLowMargin_ShouldReturnTrue_WhenBelowDefaultThreshold()
  {
    ProfitabilityCalculator.IsLowMargin(5m).Should().BeTrue();
  }

  [Fact]
  public void IsLowMargin_ShouldReturnFalse_WhenAtThreshold()
  {
    ProfitabilityCalculator.IsLowMargin(10m).Should().BeFalse();
  }

  [Fact]
  public void IsLowMargin_ShouldReturnFalse_WhenAboveThreshold()
  {
    ProfitabilityCalculator.IsLowMargin(25m).Should().BeFalse();
  }

  [Fact]
  public void IsLowMargin_ShouldReturnFalse_WhenMarginIsNegative()
  {
    // Negative margin is NOT "low" — it's handled separately
    ProfitabilityCalculator.IsLowMargin(-5m).Should().BeFalse();
  }

  [Fact]
  public void IsLowMargin_ShouldReturnTrue_WhenZeroMargin()
  {
    ProfitabilityCalculator.IsLowMargin(0m).Should().BeTrue();
  }

  [Fact]
  public void IsLowMargin_ShouldRespectCustomThreshold()
  {
    ProfitabilityCalculator.IsLowMargin(15m, threshold: 20m).Should().BeTrue();
    ProfitabilityCalculator.IsLowMargin(25m, threshold: 20m).Should().BeFalse();
  }
}

#pragma warning restore CA1707
