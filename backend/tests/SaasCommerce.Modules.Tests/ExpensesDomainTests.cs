#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class ExpensesDomainTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 22, 10, 0, 0, TimeSpan.Zero);
  private static readonly BusinessId Biz = new(Guid.NewGuid());
  private static readonly BranchId Branch = new(Guid.NewGuid());
  private static readonly Guid UserId = Guid.NewGuid();
  private static readonly Guid CategoryId = Guid.NewGuid();

  // ── ExpenseCategory.Create ───────────────────────────────────────────────

  [Fact]
  public void ExpenseCategory_Create_ShouldSucceed_WithValidData()
  {
    var category = ExpenseCategory.Create(Guid.NewGuid(), Biz, "Servicios", Now);

    category.Name.Should().Be("Servicios");
    category.IsActive.Should().BeTrue();
    category.CreatedAt.Should().Be(Now);
  }

  [Fact]
  public void ExpenseCategory_Create_ShouldThrow_WhenIdIsEmpty()
  {
    var act = () => ExpenseCategory.Create(Guid.Empty, Biz, "Servicios", Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void ExpenseCategory_Create_ShouldThrow_WhenNameIsEmpty()
  {
    var act = () => ExpenseCategory.Create(Guid.NewGuid(), Biz, "  ", Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void ExpenseCategory_Deactivate_ShouldSetIsActiveFalse()
  {
    var category = ExpenseCategory.Create(Guid.NewGuid(), Biz, "Alquiler", Now);
    category.Deactivate(Now.AddHours(1));

    category.IsActive.Should().BeFalse();
    category.UpdatedAt.Should().Be(Now.AddHours(1));
  }

  [Fact]
  public void ExpenseCategory_Activate_ShouldSetIsActiveTrue()
  {
    var category = ExpenseCategory.Create(Guid.NewGuid(), Biz, "Alquiler", Now);
    category.Deactivate(Now.AddHours(1));
    category.Activate(Now.AddHours(2));

    category.IsActive.Should().BeTrue();
    category.UpdatedAt.Should().Be(Now.AddHours(2));
  }

  // ── OperatingExpense.CreatePending ───────────────────────────────────────

  [Fact]
  public void OperatingExpense_CreatePending_ShouldSucceed_WithValidData()
  {
    var expense = NewPendingExpense();

    expense.Status.Should().Be(OperatingExpenseStatus.Pending);
    expense.Amount.Should().Be(500m);
    expense.PaymentMethod.Should().Be(ExpensePaymentMethod.Cash);
    expense.PaidAt.Should().BeNull();
    expense.CashSessionId.Should().BeNull();
    expense.CashMovementId.Should().BeNull();
    expense.CancelledAt.Should().BeNull();
  }

  [Fact]
  public void OperatingExpense_CreatePending_ShouldThrow_WhenIdIsEmpty()
  {
    var act = () => OperatingExpense.CreatePending(
      Guid.Empty, Biz, Branch, UserId, CategoryId,
      "Agua", 100m, ExpensePaymentMethod.Cash, Now, null, Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void OperatingExpense_CreatePending_ShouldThrow_WhenAmountIsZero()
  {
    var act = () => OperatingExpense.CreatePending(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "Agua", 0m, ExpensePaymentMethod.Cash, Now, null, Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void OperatingExpense_CreatePending_ShouldThrow_WhenAmountIsNegative()
  {
    var act = () => OperatingExpense.CreatePending(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "Agua", -100m, ExpensePaymentMethod.Cash, Now, null, Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void OperatingExpense_CreatePending_ShouldThrow_WhenDescriptionIsEmpty()
  {
    var act = () => OperatingExpense.CreatePending(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "  ", 100m, ExpensePaymentMethod.Cash, Now, null, Now);

    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void OperatingExpense_CreatePending_ShouldTrimDescription()
  {
    var expense = OperatingExpense.CreatePending(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "  Agua y luz  ", 100m, ExpensePaymentMethod.Cash, Now, null, Now);

    expense.Description.Should().Be("Agua y luz");
  }

  // ── OperatingExpense.CreatePaid ──────────────────────────────────────────

  [Fact]
  public void OperatingExpense_CreatePaid_ShouldSetPaidFields()
  {
    var cashSessionId = Guid.NewGuid();
    var cashMovementId = Guid.NewGuid();
    var expense = OperatingExpense.CreatePaid(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "Alquiler", 3000m, ExpensePaymentMethod.Cash,
      Now, null, cashSessionId, cashMovementId, Now);

    expense.Status.Should().Be(OperatingExpenseStatus.Paid);
    expense.PaidAt.Should().Be(Now);
    expense.PaidByUserId.Should().Be(UserId);
    expense.CashSessionId.Should().Be(cashSessionId);
    expense.CashMovementId.Should().Be(cashMovementId);
  }

  [Fact]
  public void OperatingExpense_CreatePaid_NonCash_ShouldHaveNullCashFields()
  {
    var expense = OperatingExpense.CreatePaid(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "Nómina", 50000m, ExpensePaymentMethod.Transfer,
      Now, null, null, null, Now);

    expense.Status.Should().Be(OperatingExpenseStatus.Paid);
    expense.CashSessionId.Should().BeNull();
    expense.CashMovementId.Should().BeNull();
  }

  // ── OperatingExpense.Pay ─────────────────────────────────────────────────

  [Fact]
  public void OperatingExpense_Pay_ShouldMarkAsPaid()
  {
    var expense = NewPendingExpense();
    var paidAt = Now.AddHours(1);
    var result = expense.Pay(UserId, paidAt);

    result.Should().BeTrue();
    expense.Status.Should().Be(OperatingExpenseStatus.Paid);
    expense.PaidAt.Should().Be(paidAt);
    expense.PaidByUserId.Should().Be(UserId);
    expense.UpdatedAt.Should().Be(paidAt);
  }

  [Fact]
  public void OperatingExpense_Pay_ShouldStoreCashLinks_WhenCashPayment()
  {
    var expense = NewPendingExpense();
    var sessionId = Guid.NewGuid();
    var movementId = Guid.NewGuid();
    expense.Pay(UserId, Now.AddHours(1), sessionId, movementId);

    expense.CashSessionId.Should().Be(sessionId);
    expense.CashMovementId.Should().Be(movementId);
  }

  [Fact]
  public void OperatingExpense_Pay_ShouldReturnFalse_WhenAlreadyPaid()
  {
    var expense = NewPendingExpense();
    expense.Pay(UserId, Now.AddHours(1));

    var result = expense.Pay(UserId, Now.AddHours(2));

    result.Should().BeFalse();
    expense.PaidAt.Should().Be(Now.AddHours(1)); // unchanged
  }

  [Fact]
  public void OperatingExpense_Pay_ShouldThrow_WhenCancelled()
  {
    var expense = NewPendingExpense();
    expense.Cancel(Now.AddHours(1));

    var act = () => expense.Pay(UserId, Now.AddHours(2));

    act.Should().Throw<InvalidOperationException>();
  }

  // ── OperatingExpense.Cancel ──────────────────────────────────────────────

  [Fact]
  public void OperatingExpense_Cancel_ShouldMarkAsCancelled()
  {
    var expense = NewPendingExpense();
    var cancelledAt = Now.AddHours(1);
    expense.Cancel(cancelledAt);

    expense.Status.Should().Be(OperatingExpenseStatus.Cancelled);
    expense.CancelledAt.Should().Be(cancelledAt);
    expense.UpdatedAt.Should().Be(cancelledAt);
  }

  [Fact]
  public void OperatingExpense_Cancel_ShouldBeIdempotent_WhenAlreadyCancelled()
  {
    var expense = NewPendingExpense();
    expense.Cancel(Now.AddHours(1));
    var act = () => expense.Cancel(Now.AddHours(2));

    act.Should().NotThrow();
    expense.CancelledAt.Should().Be(Now.AddHours(1)); // unchanged
  }

  [Fact]
  public void OperatingExpense_Cancel_ShouldThrow_WhenAlreadyPaid()
  {
    var expense = NewPendingExpense();
    expense.Pay(UserId, Now.AddHours(1));

    var act = () => expense.Cancel(Now.AddHours(2));

    act.Should().Throw<InvalidOperationException>();
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static OperatingExpense NewPendingExpense(decimal amount = 500m)
    => OperatingExpense.CreatePending(
      Guid.NewGuid(), Biz, Branch, UserId, CategoryId,
      "Factura de agua", amount, ExpensePaymentMethod.Cash,
      Now, null, Now);
}
