using MassTransit;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Sagas.Sales;

public sealed class SaleSagaState : SagaStateMachineInstance
{
  public Guid CorrelationId { get; set; }

  public Guid SaleId { get; set; }

  public Guid BusinessId { get; set; }

  public Guid BranchId { get; set; }

  public Guid UserId { get; set; }

  public string CurrentState { get; set; } = string.Empty;

  public DateTimeOffset CreatedAt { get; set; }

  public DateTimeOffset UpdatedAt { get; set; }

  public DateTimeOffset? CompletedAt { get; set; }

  public DateTimeOffset? FailedAt { get; set; }

  public string? FailureReason { get; set; }

  public int Version { get; set; }
}
