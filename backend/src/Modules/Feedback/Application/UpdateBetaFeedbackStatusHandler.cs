using FluentValidation;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Feedback.Application.Abstractions;
using SaasCommerce.Modules.Feedback.Contracts.Responses;
using SaasCommerce.Modules.Feedback.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Feedback.Application;

public sealed record UpdateBetaFeedbackStatusCommand(
  Guid FeedbackId,
  string Status,
  string? ReviewNote);

public sealed class UpdateBetaFeedbackStatusValidator : AbstractValidator<UpdateBetaFeedbackStatusCommand>
{
  public UpdateBetaFeedbackStatusValidator()
  {
    RuleFor(command => command.FeedbackId).NotEmpty();
    RuleFor(command => command.Status).NotEmpty().MaximumLength(50);
    RuleFor(command => command.ReviewNote).MaximumLength(1_000);
  }
}

public sealed class UpdateBetaFeedbackStatusHandler(
  IBetaFeedbackRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IValidator<UpdateBetaFeedbackStatusCommand> validator)
{
  public async Task<Result<BetaFeedbackResponse>> Handle(
    UpdateBetaFeedbackStatusCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var validation = await validator.ValidateAsync(command, cancellationToken);
    if (!validation.IsValid ||
        !Enum.TryParse<BetaFeedbackStatus>(command.Status, true, out var status))
    {
      return Result.Failure<BetaFeedbackResponse>(BetaFeedbackErrors.InvalidFeedback);
    }

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<BetaFeedbackResponse>(BetaFeedbackErrors.UserContextRequired);
    }

    var feedback = await repository.GetAsync(
      new BusinessId(businessId),
      command.FeedbackId,
      cancellationToken);

    if (feedback is null)
    {
      return Result.Failure<BetaFeedbackResponse>(BetaFeedbackErrors.FeedbackNotFound);
    }

    feedback.ChangeStatus(status, userId, clock.UtcNow, command.ReviewNote);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(BetaFeedbackResponseMapper.ToResponse(feedback));
  }
}
