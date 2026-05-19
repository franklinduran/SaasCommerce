#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class DomainEntityTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);
  private static readonly BusinessId Biz = new(Guid.NewGuid());
  private static readonly BranchId Branch = new(Guid.NewGuid());

  // ── Sale state transitions ───────────────────────────────────────────────

  private static Sale NewSale()
    => Sale.Create(Guid.NewGuid(), Biz, Branch, Guid.NewGuid(),
      [new SaleLine(Guid.NewGuid(), 1, 100)], "Cash", Now);

  [Fact]
  public void Sale_AssignCustomer_ShouldThrow_WhenCustomerIdEmpty()
  {
    var act = () => NewSale().AssignCustomer(Guid.Empty);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Sale_AssignCustomer_ShouldThrow_WhenNotReceived()
  {
    var sale = NewSale();
    sale.MarkAsProcessing(Now);
    var act = () => sale.AssignCustomer(Guid.NewGuid());
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_AssignCustomer_ShouldSucceed_WhenReceived()
  {
    var sale = NewSale();
    var cid = Guid.NewGuid();
    sale.AssignCustomer(cid);
    sale.CustomerId.Should().Be(cid);
  }

  [Fact]
  public void Sale_MarkAsProcessing_ShouldThrow_WhenCancelled()
  {
    var sale = NewSale();
    sale.Cancel("x", Now);
    var act = () => sale.MarkAsProcessing(Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_Complete_ShouldThrow_WhenNotProcessing()
  {
    var act = () => NewSale().Complete(Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_Complete_ShouldThrow_WhenCancelled()
  {
    var sale = NewSale();
    sale.Cancel("x", Now);
    var act = () => sale.Complete(Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_Fail_ShouldThrow_WhenCompleted()
  {
    var sale = NewSale();
    sale.MarkAsProcessing(Now);
    sale.Complete(Now);
    var act = () => sale.Fail("boom", Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_Fail_ShouldBeIdempotent_WhenAlreadyFailed()
  {
    var sale = NewSale();
    sale.MarkAsProcessing(Now);
    sale.Fail("first", Now);
    sale.Fail("second", Now);
    sale.Status.Should().Be(SaleStatus.Failed);
    sale.FailureReason.Should().Be("first");
  }

  [Fact]
  public void Sale_Fail_ShouldThrow_WhenReasonBlank()
  {
    var act = () => NewSale().Fail(" ", Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Sale_Cancel_ShouldThrow_WhenCompleted()
  {
    var sale = NewSale();
    sale.MarkAsProcessing(Now);
    sale.Complete(Now);
    var act = () => sale.Cancel("x", Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_Cancel_ShouldThrow_WhenFailed()
  {
    var sale = NewSale();
    sale.MarkAsProcessing(Now);
    sale.Fail("x", Now);
    var act = () => sale.Cancel("y", Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_Cancel_ShouldBeIdempotent_WhenAlreadyCancelled()
  {
    var sale = NewSale();
    sale.Cancel("first", Now);
    sale.Cancel("second", Now);
    sale.CancellationReason.Should().Be("first");
  }

  // ── Customer ─────────────────────────────────────────────────────────────

  [Fact]
  public void Customer_Ctor_ShouldThrow_WhenIdEmpty()
  {
    var act = () => new Customer(Guid.Empty, Biz, "Ana", null, null, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Customer_Ctor_ShouldThrow_WhenNameTooLong()
  {
    var act = () => new Customer(Guid.NewGuid(), Biz, new string('a', 200), null, null, Now);
    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Theory]
  [InlineData("123")]          // too short
  [InlineData("1234567890123456789012345")] // too long
  public void Customer_Ctor_ShouldThrow_WhenPhoneInvalid(string phone)
  {
    var act = () => new Customer(Guid.NewGuid(), Biz, "Ana", phone, null, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Theory]
  [InlineData("noatsign")]
  [InlineData("@startsbad.com")]
  [InlineData("endsbad@")]
  public void Customer_Ctor_ShouldThrow_WhenEmailInvalid(string email)
  {
    var act = () => new Customer(Guid.NewGuid(), Biz, "Ana", null, email, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Customer_Ctor_ShouldNormalizePhoneAndEmail()
  {
    var c = new Customer(Guid.NewGuid(), Biz, "  Ana  ", "(809) 555-0101", "ANA@TEST.COM", Now);
    c.Phone.Should().Be("8095550101");
    c.Email.Should().Be("ana@test.com");
    c.FullName.Should().Be("Ana");
    c.SearchName.Should().Be("ANA");
  }

  [Fact]
  public void Customer_Update_ShouldDeactivate_WhenIsActiveFalse()
  {
    var c = new Customer(Guid.NewGuid(), Biz, "Ana", null, null, Now);
    c.Update("Ana Maria", "8095550102", "a@b.com", isActive: false, Now);
    c.IsActive.Should().BeFalse();
    c.DeactivatedAt.Should().Be(Now);
    c.FullName.Should().Be("Ana Maria");
  }

  [Fact]
  public void Customer_Update_ShouldReactivate_WhenIsActiveTrue()
  {
    var c = new Customer(Guid.NewGuid(), Biz, "Ana", null, null, Now);
    c.Deactivate(Now);
    c.Update("Ana", null, null, isActive: true, Now);
    c.IsActive.Should().BeTrue();
    c.DeactivatedAt.Should().BeNull();
  }

  [Fact]
  public void Customer_Deactivate_ShouldBeNoop_WhenAlreadyInactive()
  {
    var c = new Customer(Guid.NewGuid(), Biz, "Ana", null, null, Now);
    c.Deactivate(Now);
    var firstDeactivatedAt = c.DeactivatedAt;
    c.Deactivate(Now.AddDays(1));
    c.DeactivatedAt.Should().Be(firstDeactivatedAt);
  }

  // ── Supplier ─────────────────────────────────────────────────────────────

  [Fact]
  public void Supplier_Ctor_ShouldThrow_WhenIdEmpty()
  {
    var act = () => new Supplier(Guid.Empty, Biz, "Prov", new SupplierContactInfo(null, null, null, null), Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Supplier_Ctor_ShouldThrow_WhenNameTooLong()
  {
    var act = () => new Supplier(Guid.NewGuid(), Biz, new string('x', 200),
      new SupplierContactInfo(null, null, null, null), Now);
    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Supplier_Ctor_ShouldThrow_WhenEmailInvalid()
  {
    var act = () => new Supplier(Guid.NewGuid(), Biz, "Prov",
      new SupplierContactInfo(null, null, "bademail", null), Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Supplier_Ctor_ShouldNormalizeContactInfo()
  {
    var s = new Supplier(Guid.NewGuid(), Biz, " Distribuidora ",
      new SupplierContactInfo("RNC-101-222", "(809) 111-2222", "VENTAS@PROV.COM", "  Calle 1  "), Now);
    s.Rnc.Should().Be("101222");
    s.Phone.Should().Be("8091112222");
    s.Email.Should().Be("ventas@prov.com");
    s.Address.Should().Be("Calle 1");
    s.SearchName.Should().Be("DISTRIBUIDORA");
  }

  [Fact]
  public void Supplier_Update_ShouldApplyChangesAndStatus()
  {
    var s = new Supplier(Guid.NewGuid(), Biz, "Prov",
      new SupplierContactInfo(null, null, null, null), Now);
    s.Update("Nuevo", "999", "8090000000", "n@p.com", "Dir", isActive: false, Now);
    s.Name.Should().Be("Nuevo");
    s.IsActive.Should().BeFalse();
    s.UpdatedAt.Should().Be(Now);
  }

  // ── CustomerCreditAccount ────────────────────────────────────────────────

  [Fact]
  public void CreditAccount_Ctor_ShouldThrow_WhenIdEmpty()
  {
    var act = () => new CustomerCreditAccount(Guid.Empty, Biz, Guid.NewGuid(), 0, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditAccount_Ctor_ShouldThrow_WhenCustomerEmpty()
  {
    var act = () => new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.Empty, 0, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditAccount_Ctor_ShouldThrow_WhenLimitNegative()
  {
    var act = () => new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), -1, Now);
    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void CreditAccount_ApplyDebit_ShouldThrow_WhenSaleIdEmpty()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 1000, Now);
    var act = () => acc.ApplyDebit(Guid.NewGuid(), Guid.Empty, 10, null, null, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditAccount_ApplyPayment_ShouldThrow_WhenPaymentIdEmpty()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 1000, Now);
    acc.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 100, null, null, Now);
    var act = () => acc.ApplyPayment(Guid.NewGuid(), Guid.Empty, 50, null, null, Now);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditAccount_ApplyPayment_ShouldThrow_WhenExceedsBalance()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 1000, Now);
    acc.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 100, null, null, Now);
    var act = () => acc.ApplyPayment(Guid.NewGuid(), Guid.NewGuid(), 500, null, null, Now);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CreditAccount_EnsureCanDebit_ShouldThrow_WhenBlocked()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 1000, Now);
    acc.Block(Now);
    var act = () => acc.EnsureCanDebit(10);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CreditAccount_EnsureCanDebit_ShouldThrow_WhenLimitExceeded()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 100, Now);
    var act = () => acc.EnsureCanDebit(500);
    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CreditAccount_BlockUnblock_ShouldToggleStatus()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 1000, Now);
    acc.Block(Now);
    acc.Status.Should().Be(CustomerCreditStatus.Blocked);
    acc.Unblock(Now);
    acc.Status.Should().Be(CustomerCreditStatus.Active);
  }

  [Fact]
  public void CreditAccount_ApplyPayment_ShouldReduceBalance()
  {
    var acc = new CustomerCreditAccount(Guid.NewGuid(), Biz, Guid.NewGuid(), 1000, Now);
    acc.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 300, "venta", null, Now);
    var movement = acc.ApplyPayment(Guid.NewGuid(), Guid.NewGuid(), 120, "abono", null, Now);
    acc.CurrentBalance.Should().Be(180);
    movement.Type.Should().Be(CustomerCreditMovementType.Payment);
  }

  // ── CustomerCreditMovement ───────────────────────────────────────────────

  private static CreditMovementIdentifiers Ids()
    => new(Guid.NewGuid(), Biz, Guid.NewGuid(), new CreditMovementSource(SaleId: Guid.NewGuid()));

  [Fact]
  public void CreditMovement_ShouldThrow_WhenIdEmpty()
  {
    var ids = new CreditMovementIdentifiers(Guid.Empty, Biz, Guid.NewGuid(), new CreditMovementSource());
    var act = () => new CustomerCreditMovement(ids, CustomerCreditMovementType.Debit, 10,
      new CreditMovementBalance(0, 10), null, Now, null);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditMovement_ShouldThrow_WhenCustomerEmpty()
  {
    var ids = new CreditMovementIdentifiers(Guid.NewGuid(), Biz, Guid.Empty, new CreditMovementSource());
    var act = () => new CustomerCreditMovement(ids, CustomerCreditMovementType.Debit, 10,
      new CreditMovementBalance(0, 10), null, Now, null);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditMovement_ShouldThrow_WhenAmountNotPositive()
  {
    var act = () => new CustomerCreditMovement(Ids(), CustomerCreditMovementType.Debit, 0,
      new CreditMovementBalance(0, 0), null, Now, null);
    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void CreditMovement_ShouldThrow_WhenBalanceNegative()
  {
    var act = () => new CustomerCreditMovement(Ids(), CustomerCreditMovementType.Debit, 10,
      new CreditMovementBalance(-1, 10), null, Now, null);
    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void CreditMovement_ShouldThrow_WhenBalanceInconsistent()
  {
    var act = () => new CustomerCreditMovement(Ids(), CustomerCreditMovementType.Debit, 10,
      new CreditMovementBalance(0, 999), null, Now, null);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void CreditMovement_ShouldTruncateLongNote()
  {
    var movement = new CustomerCreditMovement(Ids(), CustomerCreditMovementType.Debit, 10,
      new CreditMovementBalance(0, 10), new string('n', 800), Now, Guid.NewGuid());
    movement.Note!.Length.Should().Be(500);
  }

  [Fact]
  public void CreditMovement_Payment_ShouldAllowConsistentBalance()
  {
    var movement = new CustomerCreditMovement(Ids(), CustomerCreditMovementType.Payment, 40,
      new CreditMovementBalance(100, 60), "  abono  ", Now, null);
    movement.Note.Should().Be("abono");
    movement.NewBalance.Should().Be(60);
  }

  [Fact]
  public void CreditMovement_Adjustment_ShouldAllowAnyBalance()
  {
    var movement = new CustomerCreditMovement(Ids(), CustomerCreditMovementType.Adjustment, 5,
      new CreditMovementBalance(100, 250), null, Now, null);
    movement.Type.Should().Be(CustomerCreditMovementType.Adjustment);
  }
}

#pragma warning restore CA1707
