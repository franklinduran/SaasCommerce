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

public sealed record CreateBetaFeedbackCommand(
  string Category,
  string Title,
  string Description,
  string? ContextUrl);

public sealed class CreateBetaFeedbackValidator : AbstractValidator<CreateBetaFeedbackCommand>
{
  public CreateBetaFeedbackValidator()
  {
    RuleFor(command => command.Category).NotEmpty().MaximumLength(50);
    RuleFor(command => command.Title).NotEmpty().MaximumLength(160);
    RuleFor(command => command.Description).NotEmpty().MaximumLength(2_000);
    RuleFor(command => command.ContextUrl).MaximumLength(500);
  }
}

public sealed class CreateBetaFeedbackHandler(
  IBetaFeedbackRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  IValidator<CreateBetaFeedbackCommand> validator)
{
  public async Task<Result<BetaFeedbackResponse>> Handle(
    CreateBetaFeedbackCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var validation = await validator.ValidateAsync(command, cancellationToken);
    if (!validation.IsValid ||
        !Enum.TryParse<BetaFeedbackCategory>(command.Category, true, out var category))
    {
      return Result.Failure<BetaFeedbackResponse>(BetaFeedbackErrors.InvalidFeedback);
    }

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<BetaFeedbackResponse>(BetaFeedbackErrors.UserContextRequired);
    }

    var feedback = BetaFeedback.Create(new BetaFeedbackDraft(
      Guid.NewGuid(),
      new BusinessId(businessId),
      userId,
      category,
      command.Title,
      command.Description,
      command.ContextUrl,
      clock.UtcNow));

    await repository.AddAsync(feedback, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(BetaFeedbackResponseMapper.ToResponse(feedback));
  }
}
