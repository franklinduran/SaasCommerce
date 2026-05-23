#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CashDomainTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 22, 10, 0, 0, TimeSpan.Zero);
  private static readonly BusinessId Biz = new(Guid.NewGuid());
  private static readonly BranchId Branch = new(Guid.NewGuid());
  private static readonly Guid UserId = Guid.NewGuid();

  // ── CashSession.Create ───────────────────────────────────────────────────

  [Fact]
  public void CashSession_Create_ShouldSucceed_WithZeroOpeningBalance()
  {
    var session = NewSession(openingBalance: 0);

    session.OpeningBalance.Should().Be(0);
    session.Status.Should().Be(CashSessionStatus.Open);
    session.SystemBalance.Should().Be(0);
  }

  [Fact]
  public void CashSession_Create_ShouldThrow_WhenOpeningBalanceIsNegative()
  {
    var act = () => NewSession(openingBalance: -1);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashSession_Create_ShouldThrow_WhenIdIsEmpty()
  {
    var act = () => CashSession.Create(Guid.Empty, Biz, Branch, UserId, 100, null, Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashSession_Create_ShouldThrow_WhenUserIdIsEmpty()
  {
    var act = () => CashSession.Create(Guid.NewGuid(), Biz, Branch, Guid.Empty, 100, null, Now);

    act.Should().Throw<ArgumentException>();
  }

  // ── CashSession.SystemBalance ────────────────────────────────────────────

  [Fact]
  public void CashSession_SystemBalance_ShouldReflectCashIn()
  {
    var session = NewSession(openingBalance: 500);
    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashIn, 200, "Ingreso", Now);

    session.SystemBalance.Should().Be(700);
  }

  [Fact]
  public void CashSession_SystemBalance_ShouldReflectCashOut()
  {
    var session = NewSession(openingBalance: 500);
    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashOut, 100, "Retiro", Now);

    session.SystemBalance.Should().Be(400);
  }

  [Fact]
  public void CashSession_SystemBalance_ShouldAccumulateMultipleMovements()
  {
    var session = NewSession(openingBalance: 1000);
    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashIn, 500, "Entrada 1", Now);
    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashOut, 300, "Salida 1", Now);
    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashIn, 200, "Entrada 2", Now);

    // 1000 + 500 - 300 + 200 = 1400
    session.SystemBalance.Should().Be(1400);
  }

  // ── CashSession.AddMovement ──────────────────────────────────────────────

  [Fact]
  public void CashSession_AddMovement_ShouldAddToMovementsCollection()
  {
    var session = NewSession();

    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashIn, 150, "Test", Now);

    session.Movements.Should().ContainSingle();
    session.Movements.First().Amount.Should().Be(150);
    session.Movements.First().Type.Should().Be(CashMovementType.CashIn);
  }

  [Fact]
  public void CashSession_AddMovement_ShouldThrow_WhenSessionIsClosed()
  {
    var session = NewSession(openingBalance: 500);
    session.Close(500, Now.AddMinutes(30));

    var act = () => session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashIn, 100, "Late entry", Now.AddMinutes(31));

    act.Should().Throw<InvalidOperationException>();
  }

  // ── CashSession.Close ────────────────────────────────────────────────────

  [Fact]
  public void CashSession_Close_ShouldReturnBalanced_WhenClosingMatchesSystem()
  {
    var session = NewSession(openingBalance: 1000);
    session.AddMovement(Guid.NewGuid(), UserId, CashMovementType.CashIn, 500, "Venta", Now);
    // System = 1500

    var result = session.Close(closingBalance: 1500, Now.AddMinutes(60));

    result.Outcome.Should().Be("Balanced");
    result.Difference.Should().Be(0);
    result.SystemBalance.Should().Be(1500);
    result.ClosingBalance.Should().Be(1500);
  }

  [Fact]
  public void CashSession_Close_ShouldReturnSurplus_WhenClosingExceedsSystem()
  {
    var session = NewSession(openingBalance: 1000);
    // System = 1000

    var result = session.Close(closingBalance: 1100, Now.AddMinutes(60));

    result.Outcome.Should().Be("Surplus");
    result.Difference.Should().Be(100);
  }

  [Fact]
  public void CashSession_Close_ShouldReturnShortage_WhenClosingBelowSystem()
  {
    var session = NewSession(openingBalance: 1000);
    // System = 1000

    var result = session.Close(closingBalance: 900, Now.AddMinutes(60));

    result.Outcome.Should().Be("Shortage");
    result.Difference.Should().Be(-100);
  }

  [Fact]
  public void CashSession_Close_ShouldSetStatusToClosed()
  {
    var session = NewSession(openingBalance: 500);

    session.Close(500, Now.AddMinutes(30));

    session.Status.Should().Be(CashSessionStatus.Closed);
    session.ClosedAt.Should().Be(Now.AddMinutes(30));
    session.ClosingBalance.Should().Be(500);
  }

  [Fact]
  public void CashSession_Close_ShouldThrow_WhenAlreadyClosed()
  {
    var session = NewSession(openingBalance: 500);
    session.Close(500, Now.AddMinutes(30));

    var act = () => session.Close(500, Now.AddMinutes(60));

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CashSession_Close_ShouldThrow_WhenClosingBalanceIsNegative()
  {
    var session = NewSession(openingBalance: 500);

    var act = () => session.Close(closingBalance: -1, Now.AddMinutes(30));

    act.Should().Throw<ArgumentException>();
  }

  // ── CashMovement.Create ──────────────────────────────────────────────────

  [Fact]
  public void CashMovement_Create_ShouldSucceed_WithValidData()
  {
    var movement = CashMovement.Create(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      CashMovementType.CashIn, 250, "Apertura extra", Now);

    movement.Amount.Should().Be(250);
    movement.Type.Should().Be(CashMovementType.CashIn);
    movement.Description.Should().Be("Apertura extra");
  }

  [Fact]
  public void CashMovement_Create_ShouldThrow_WhenAmountIsZero()
  {
    var act = () => CashMovement.Create(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      CashMovementType.CashIn, 0, "Test", Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashMovement_Create_ShouldThrow_WhenAmountIsNegative()
  {
    var act = () => CashMovement.Create(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      CashMovementType.CashOut, -50, "Test", Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashMovement_Create_ShouldThrow_WhenDescriptionIsEmpty()
  {
    var act = () => CashMovement.Create(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      CashMovementType.CashIn, 100, "", Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashMovement_Create_ShouldThrow_WhenDescriptionIsWhitespace()
  {
    var act = () => CashMovement.Create(
      Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
      CashMovementType.CashIn, 100, "   ", Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CashMovement_Create_ShouldThrow_WhenMovementIdIsEmpty()
  {
    var act = () => CashMovement.Create(
      Guid.Empty, Guid.NewGuid(), Guid.NewGuid(),
      CashMovementType.CashIn, 100, "Test", Now);

    act.Should().Throw<ArgumentException>();
  }

  // ── CashSessionClosingResult ─────────────────────────────────────────────

  [Theory]
  [InlineData(0, "Balanced")]
  [InlineData(50, "Surplus")]
  [InlineData(-50, "Shortage")]
  public void CashSessionClosingResult_Outcome_ShouldMatchDifference(decimal difference, string expectedOutcome)
  {
    var result = new CashSessionClosingResult(1000, 1000, 1000 + difference, difference);

    result.Outcome.Should().Be(expectedOutcome);
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static CashSession NewSession(decimal openingBalance = 500)
    => CashSession.Create(Guid.NewGuid(), Biz, Branch, UserId, openingBalance, null, Now);
}

#pragma warning restore CA1707
