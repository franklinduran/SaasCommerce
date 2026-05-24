namespace SaasCommerce.Modules.Sales.Domain;

public sealed class DailyClosingAlert
{
  private DailyClosingAlert()
  {
  }

  private DailyClosingAlert(
    Guid id,
    Guid dailyClosingId,
    DailyClosingAlertType alertType,
    string message,
    decimal? estimatedImpact)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Alert id is required.", nameof(id));
    }

    if (dailyClosingId == Guid.Empty)
    {
      throw new ArgumentException("DailyClosing id is required.", nameof(dailyClosingId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(message);

    Id = id;
    DailyClosingId = dailyClosingId;
    AlertType = alertType;
    Message = message.Trim();
    EstimatedImpact = estimatedImpact;
  }

  public Guid Id { get; private set; }

  public Guid DailyClosingId { get; private set; }

  public DailyClosingAlertType AlertType { get; private set; }

  public string Message { get; private set; } = string.Empty;

  public decimal? EstimatedImpact { get; private set; }

  public static DailyClosingAlert Create(
    Guid id,
    Guid dailyClosingId,
    DailyClosingAlertType alertType,
    string message,
    decimal? estimatedImpact = null)
    => new(id, dailyClosingId, alertType, message, estimatedImpact);
}
