using FluentValidation;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public sealed record GetCashRegistersQuery(
  Guid? BranchId,
  string? Status,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize);

public sealed class GetCashRegistersValidator : AbstractValidator<GetCashRegistersQuery>
{
  public GetCashRegistersValidator()
  {
    RuleFor(x => x.Page).GreaterThan(0);
    RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
  }
}

public sealed class GetCashRegistersHandler(
  ICashRegisterReadRepository cashRegisters,
  ICurrentUserService currentUser)
{
  public async Task<Result<CashRegisterListResponse>> Handle(
    GetCashRegistersQuery query,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId)
    {
      return Result.Failure<CashRegisterListResponse>(CashRegisterErrors.UserContextRequired);
    }

    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, 100);
    var criteria = new CashRegisterSearchCriteria(
      query.BranchId,
      query.Status,
      query.DateFrom,
      query.DateTo,
      page,
      pageSize);

    var businessId = new BusinessId(rawBusinessId);
    var items = await cashRegisters.ListAsync(businessId, criteria, cancellationToken);
    var total = await cashRegisters.CountAsync(businessId, criteria, cancellationToken);

    return Result.Success(new CashRegisterListResponse(
      items.Select(CashRegisterResponseMapper.ToSummary).ToArray(),
      page,
      pageSize,
      total));
  }
}

public sealed record CashRegisterListResponse(
  IReadOnlyCollection<CashRegisterSummaryResponse> Items,
  int Page,
  int PageSize,
  int TotalCount);

public sealed record CashRegisterSummaryResponse(
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
