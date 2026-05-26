#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CashRegisterDomainTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 26, 8, 0, 0, TimeSpan.Zero);

  private static CashRegister CreateOpenRegister(decimal openingAmount = 500)
    => CashRegister.Open(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      openingAmount,
      null,
      Now);

  private static readonly CashRegisterTotals ZeroTotals = new(0, 0, 0, 0, 0);

  [Fact]
  public void CashRegister_ShouldOpen_WhenOpeningAmountIsValid()
  {
    var register = CreateOpenRegister(openingAmount: 1000);

    register.Status.Should().Be(CashRegisterStatus.Open);
    register.OpeningAmount.Should().Be(1000);
    register.OpenedAt.Should().Be(Now);
    register.ClosedAt.Should().BeNull();
  }

  [Fact]
  public void CashRegister_ShouldOpen_WithZeroOpeningAmount()
  {
    var register = CreateOpenRegister(openingAmount: 0);

    register.Status.Should().Be(CashRegisterStatus.Open);
    register.OpeningAmount.Should().Be(0);
  }

  [Fact]
  public void CashRegister_ShouldRejectNegativeOpeningAmount()
  {
    var act = () => CashRegister.Open(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      -1,
      null,
      Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashRegister_ShouldAddMovement_WhenOpen()
  {
    var register = CreateOpenRegister();

    var movement = register.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashIn, 200, "Depósito", Now);

    register.Movements.Should().ContainSingle();
    movement.Amount.Should().Be(200);
    movement.MovementType.Should().Be(CashMovementType.CashIn);
    register.ManualCashIn.Should().Be(200);
  }

  [Fact]
  public void CashRegister_ShouldRejectMovement_WhenClosed()
  {
    var register = CreateOpenRegister();
    register.Close(500, ZeroTotals, Now.AddHours(8), null);

    var act = () => register.AddMovement(
      Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashIn, 100, "Late", Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CashRegister_ShouldClose_WhenCountedAmountIsProvided()
  {
    var register = CreateOpenRegister(openingAmount: 1000);

    var result = register.Close(1000, ZeroTotals, Now.AddHours(8), null);

    register.Status.Should().Be(CashRegisterStatus.Closed);
    register.ClosedAt.Should().NotBeNull();
    result.DifferenceType.Should().Be(CashDifferenceType.Balanced);
    result.Difference.Should().Be(0);
    result.ExpectedCashAmount.Should().Be(1000);
  }

  [Fact]
  public void CashRegister_ShouldCalculateShortage_WhenCountedAmountIsLessThanExpected()
  {
    var register = CreateOpenRegister(openingAmount: 1000);
    var totals = new CashRegisterTotals(CashSales: 500, 0, 0, 0, CashReturns: 0);

    // Expected = 1000 + 500 = 1500; Counted = 1400 → Shortage -100
    var result = register.Close(1400, totals, Now.AddHours(8), null);

    result.DifferenceType.Should().Be(CashDifferenceType.Shortage);
    result.Difference.Should().Be(-100);
    result.ExpectedCashAmount.Should().Be(1500);
  }

  [Fact]
  public void CashRegister_ShouldCalculateSurplus_WhenCountedAmountIsGreaterThanExpected()
  {
    var register = CreateOpenRegister(openingAmount: 1000);
    var totals = new CashRegisterTotals(CashSales: 500, 0, 0, 0, CashReturns: 0);

    // Expected = 1000 + 500 = 1500; Counted = 1600 → Surplus +100
    var result = register.Close(1600, totals, Now.AddHours(8), null);

    result.DifferenceType.Should().Be(CashDifferenceType.Surplus);
    result.Difference.Should().Be(100);
    result.ExpectedCashAmount.Should().Be(1500);
  }

  [Fact]
  public void CashRegister_ShouldComputeManualCashIn_FromMovements()
  {
    var register = CreateOpenRegister();
    register.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashIn, 300, "Depósito 1", Now);
    register.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashIn, 200, "Depósito 2", Now.AddMinutes(1));

    register.ManualCashIn.Should().Be(500);
    register.ManualCashOut.Should().Be(0);
  }

  [Fact]
  public void CashRegister_ShouldComputeManualCashOut_FromMovements()
  {
    var register = CreateOpenRegister();
    register.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashOut, 150, "Retiro", Now);

    register.ManualCashOut.Should().Be(150);
    register.ManualCashIn.Should().Be(0);
  }

  [Fact]
  public void CashRegister_ExpectedCashAmount_IncludesMovementsAndSales()
  {
    var register = CreateOpenRegister(openingAmount: 500);
    register.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashIn, 100, "In", Now);
    register.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashOut, 50, "Out", Now.AddMinutes(1));
    var totals = new CashRegisterTotals(CashSales: 300, 0, 0, 0, CashReturns: 100);

    // Expected = 500 + 300 - 100 + 100 - 50 = 750
    var result = register.Close(750, totals, Now.AddHours(8), null);

    result.ExpectedCashAmount.Should().Be(750);
    result.DifferenceType.Should().Be(CashDifferenceType.Balanced);
    result.ManualCashIn.Should().Be(100);
    result.ManualCashOut.Should().Be(50);
  }

  [Fact]
  public void CashRegister_ShouldRejectClose_WhenAlreadyClosed()
  {
    var register = CreateOpenRegister();
    register.Close(500, ZeroTotals, Now.AddHours(8), null);

    var act = () => register.Close(500, ZeroTotals, Now.AddHours(9), null);

    act.Should().Throw<InvalidOperationException>();
  }
}

#pragma warning restore CA1707
