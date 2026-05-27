using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Feedback.Application.Abstractions;
using SaasCommerce.Modules.Feedback.Contracts.Responses;
using SaasCommerce.Modules.Feedback.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Feedback.Application;

public sealed record GetBetaFeedbackQuery(
  string? Status,
  string? Category,
  int Page = 1,
  int PageSize = 20);

public sealed class GetBetaFeedbackHandler(
  IBetaFeedbackRepository repository,
  ICurrentUserService currentUser)
{
  private static readonly int[] AllowedPageSizes = [10, 20, 50];

  public async Task<Result<BetaFeedbackListResponse>> Handle(
    GetBetaFeedbackQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<BetaFeedbackListResponse>(BetaFeedbackErrors.UserContextRequired);
    }

    if (!TryBuildCriteria(query, out var criteria))
    {
      return Result.Failure<BetaFeedbackListResponse>(BetaFeedbackErrors.InvalidFeedback);
    }

    var tenantId = new BusinessId(businessId);
    var totalItems = await repository.CountAsync(tenantId, criteria, cancellationToken);
    var items = await repository.ListAsync(tenantId, criteria, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)criteria.PageSize);

    return Result.Success(new BetaFeedbackListResponse(
      items.Select(BetaFeedbackResponseMapper.ToResponse).ToArray(),
      criteria.Page,
      criteria.PageSize,
      totalItems,
      totalPages,
      criteria.Page > 1,
      totalPages > 0 && criteria.Page < totalPages));
  }

  private static bool TryBuildCriteria(
    GetBetaFeedbackQuery query,
    out BetaFeedbackSearchCriteria criteria)
  {
    criteria = new BetaFeedbackSearchCriteria(null, null, query.Page, query.PageSize);

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize))
    {
      return false;
    }

    if (!TryParseEnum(query.Status, out BetaFeedbackStatus? status) ||
        !TryParseEnum(query.Category, out BetaFeedbackCategory? category))
    {
      return false;
    }

    criteria = new BetaFeedbackSearchCriteria(status, category, query.Page, query.PageSize);
    return true;
  }

  private static bool TryParseEnum<TEnum>(string? value, out TEnum? parsed)
    where TEnum : struct
  {
    parsed = null;
    if (string.IsNullOrWhiteSpace(value))
    {
      return true;
    }

    if (!Enum.TryParse<TEnum>(value, true, out var result))
    {
      return false;
    }

    parsed = result;
    return true;
  }
}
