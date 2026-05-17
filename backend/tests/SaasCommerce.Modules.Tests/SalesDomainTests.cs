#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class SalesDomainTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);

  [Fact]
  public void Sale_ShouldMoveToProcessing_WhenStateIsReceived()
  {
    var sale = CreateSale();

    sale.MarkAsProcessing(Now.AddMinutes(1));

    sale.Status.Should().Be(SaleStatus.Processing);
  }

  [Fact]
  public void Sale_ShouldComplete_WhenStateIsProcessing()
  {
    var sale = CreateSale();
    sale.MarkAsProcessing(Now.AddMinutes(1));

    sale.Complete(Now.AddMinutes(2));

    sale.Status.Should().Be(SaleStatus.Completed);
    sale.CompletedAt.Should().Be(Now.AddMinutes(2));
  }

  [Fact]
  public void Sale_ShouldThrow_WhenCompletingFromFailed()
  {
    var sale = CreateSale();
    sale.MarkAsProcessing(Now.AddMinutes(1));
    sale.Fail("stock failed", Now.AddMinutes(2));

    var action = () => sale.Complete(Now.AddMinutes(3));

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_ShouldThrow_WhenFailingCompletedSale()
  {
    var sale = CreateSale();
    sale.MarkAsProcessing(Now.AddMinutes(1));
    sale.Complete(Now.AddMinutes(2));

    var action = () => sale.Fail("late failure", Now.AddMinutes(3));

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void Sale_ShouldThrow_WhenCancellingCompletedSale()
  {
    var sale = CreateSale();
    sale.MarkAsProcessing(Now.AddMinutes(1));
    sale.Complete(Now.AddMinutes(2));

    var action = () => sale.Cancel("customer cancelled", Now.AddMinutes(3));

    action.Should().Throw<InvalidOperationException>();
  }

  private static Sale CreateSale()
    => Sale.Create(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      [new SaleLine(Guid.NewGuid(), 2, 125)],
      "Cash",
      Now);
}

#pragma warning restore CA1707
