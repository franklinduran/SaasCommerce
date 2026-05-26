using FluentValidation;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public sealed record GetDailyCashRegisterSummaryQuery(DateOnly SummaryDate, Guid? BranchId);

public sealed class GetDailyCashRegisterSummaryValidator : AbstractValidator<GetDailyCashRegisterSummaryQuery>
{
  public GetDailyCashRegisterSummaryValidator()
  {
    RuleFor(x => x.SummaryDate).NotEmpty().WithMessage("La fecha es requerida.");
  }
}

public sealed class GetDailyCashRegisterSummaryHandler(
  ICashRegisterRepository cashRegisters,
  ICurrentUserService currentUser)
{
  public async Task<Result<DailyCashRegisterSummaryResponse>> Handle(
    GetDailyCashRegisterSummaryQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId)
    {
      return Result.Failure<DailyCashRegisterSummaryResponse>(CashRegisterErrors.UserContextRequired);
    }

    var businessId = new BusinessId(rawBusinessId);
    var found = await cashRegisters.GetDailySummaryAsync(
      businessId,
      query.BranchId,
      query.SummaryDate,
      cancellationToken);

    var items = found.Select(r => new DailyCashRegisterSummaryItem(
      r.Id,
      r.BranchId.Value,
      r.UserId,
      r.OpeningAmount,
      r.Status.ToString(),
      r.OpenedAt,
      r.ClosedAt,
      r.ExpectedCashAmount,
      r.CountedAmount,
      r.Difference,
      r.DifferenceType?.ToString(),
      r.CashSalesTotal,
      r.CardSalesTotal,
      r.TransferSalesTotal,
      r.CreditSalesTotal,
      r.CashReturnsTotal,
      r.ManualCashIn,
      r.ManualCashOut))
      .ToArray();

    var summary = new DailyCashRegisterSummaryResponse(
      query.SummaryDate,
      items.Count(i => i.Status == CashRegisterStatus.Open.ToString()),
      items.Count(i => i.Status == CashRegisterStatus.Closed.ToString()),
      items.Sum(i => i.ExpectedCashAmount ?? 0),
      items.Sum(i => i.CountedAmount ?? 0),
      items.Sum(i => i.Difference ?? 0),
      items.Sum(i => i.CashSalesTotal),
      items.Sum(i => i.CardSalesTotal),
      items.Sum(i => i.TransferSalesTotal),
      items.Sum(i => i.CreditSalesTotal),
      items.Sum(i => i.CashReturnsTotal),
      items);

    return Result.Success(summary);
  }
}

public sealed record DailyCashRegisterSummaryItem(
  Guid CashRegisterId,
  Guid BranchId,
  Guid UserId,
  decimal OpeningAmount,
  string Status,
  DateTimeOffset OpenedAt,
  DateTimeOffset? ClosedAt,
  decimal? ExpectedCashAmount,
  decimal? CountedAmount,
  decimal? Difference,
  string? DifferenceType,
  decimal CashSalesTotal,
  decimal CardSalesTotal,
  decimal TransferSalesTotal,
  decimal CreditSalesTotal,
  decimal CashReturnsTotal,
  decimal ManualCashIn,
  decimal ManualCashOut);

public sealed record DailyCashRegisterSummaryResponse(
  DateOnly Date,
  int OpenRegisters,
  int ClosedRegisters,
  decimal TotalExpectedCash,
  decimal TotalCountedCash,
  decimal TotalDifference,
  decimal TotalCashSales,
  decimal TotalCardSales,
  decimal TotalTransferSales,
  decimal TotalCreditSales,
  decimal TotalCashReturns,
  IReadOnlyCollection<DailyCashRegisterSummaryItem> Registers);
