using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Feedback.Domain;

public sealed class BetaFeedback
{
  private BetaFeedback()
  {
  }

  private BetaFeedback(BetaFeedbackDraft draft)
  {
    ValidateDraft(draft);

    Id = draft.Id;
    BusinessId = draft.BusinessId;
    UserId = draft.UserId;
    Category = draft.Category;
    Status = BetaFeedbackStatus.New;
    Title = draft.Title.Trim();
    Description = draft.Description.Trim();
    ContextUrl = string.IsNullOrWhiteSpace(draft.ContextUrl) ? null : draft.ContextUrl.Trim();
    CreatedAt = draft.CreatedAt;
    UpdatedAt = draft.CreatedAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid UserId { get; private set; }

  public BetaFeedbackCategory Category { get; private set; }

  public BetaFeedbackStatus Status { get; private set; }

  public string Title { get; private set; } = string.Empty;

  public string Description { get; private set; } = string.Empty;

  public string? ContextUrl { get; private set; }

  public string? ReviewNote { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public DateTimeOffset? ReviewedAt { get; private set; }

  public Guid? ReviewedByUserId { get; private set; }

  public static BetaFeedback Create(BetaFeedbackDraft draft) => new(draft);

  public void ChangeStatus(
    BetaFeedbackStatus status,
    Guid reviewerUserId,
    DateTimeOffset changedAt,
    string? reviewNote)
  {
    if (reviewerUserId == Guid.Empty)
    {
      throw new ArgumentException("Reviewer user id is required.", nameof(reviewerUserId));
    }

    Status = status;
    ReviewNote = string.IsNullOrWhiteSpace(reviewNote) ? null : reviewNote.Trim();
    ReviewedAt = changedAt;
    ReviewedByUserId = reviewerUserId;
    UpdatedAt = changedAt;
  }

  private static void ValidateDraft(BetaFeedbackDraft draft)
  {
    ArgumentNullException.ThrowIfNull(draft);

    if (draft.Id == Guid.Empty)
    {
      throw new ArgumentException("Feedback id is required.", nameof(draft));
    }

    if (draft.UserId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(draft));
    }

    if (string.IsNullOrWhiteSpace(draft.Title))
    {
      throw new ArgumentException("Title is required.", nameof(draft));
    }

    if (string.IsNullOrWhiteSpace(draft.Description))
    {
      throw new ArgumentException("Description is required.", nameof(draft));
    }
  }
}

public sealed record BetaFeedbackDraft(
  Guid Id,
  BusinessId BusinessId,
  Guid UserId,
  BetaFeedbackCategory Category,
  string Title,
  string Description,
  string? ContextUrl,
  DateTimeOffset CreatedAt);
