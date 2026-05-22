using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Subscriptions.Mappers;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Billing.Application.Subscriptions.Plans;

public sealed class GetSubscriptionPlanByIdQueryHandler(ISubscriptionPlanRepository planRepository)
{
  public async Task<Result<SubscriptionPlanResponse>> Handle(
    GetSubscriptionPlanByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    var plan = await planRepository.GetByIdAsync(query.PlanId, cancellationToken);

    return plan is null
      ? Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.PlanNotFound)
      : Result.Success(SubscriptionPlanResponseMapper.ToResponse(plan));
  }
}

public sealed class CreateSubscriptionPlanCommandHandler(
  ISubscriptionPlanRepository planRepository,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<SubscriptionPlanResponse>> Handle(
    CreateSubscriptionPlanCommand command,
    CancellationToken cancellationToken = default)
  {
    var duplicate = await planRepository.GetByCodeAsync(command.Code, cancellationToken);
    if (duplicate is not null)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.DuplicatePlanCode);
    }

    SubscriptionPlan plan;
    try
    {
      plan = SubscriptionPlan.Create(
        Guid.NewGuid(),
        command.Name,
        command.Code,
        command.Description ?? string.Empty,
        command.MonthlyPrice,
        command.MaxBranches,
        command.MaxUsers,
        command.MaxProducts,
        command.MaxSalesPerMonth,
        BuildFeatures(
          command.AllowInventoryTransfers,
          command.AllowAdvancedReports,
          command.AllowAuditLogs),
        clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.InvalidPlanData);
    }

    await planRepository.AddAsync(plan, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SubscriptionPlanResponseMapper.ToResponse(plan));
  }

  private static SubscriptionFeature BuildFeatures(
    bool allowInventoryTransfers,
    bool allowAdvancedReports,
    bool allowAuditLogs)
    => SubscriptionPlanFeatureBuilder.Build(
      allowInventoryTransfers,
      allowAdvancedReports,
      allowAuditLogs);
}

public sealed class UpdateSubscriptionPlanCommandHandler(
  ISubscriptionPlanRepository planRepository,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<SubscriptionPlanResponse>> Handle(
    UpdateSubscriptionPlanCommand command,
    CancellationToken cancellationToken = default)
  {
    var plan = await planRepository.GetByIdAsync(command.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.PlanNotFound);
    }

    var duplicate = await planRepository.GetByCodeAsync(command.Code, cancellationToken);
    if (duplicate is not null && duplicate.Id != plan.Id)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.DuplicatePlanCode);
    }

    try
    {
      plan.Update(
        command.Name,
        command.Code,
        command.Description ?? string.Empty,
        command.MonthlyPrice,
        command.MaxBranches,
        command.MaxUsers,
        command.MaxProducts,
        command.MaxSalesPerMonth,
        SubscriptionPlanFeatureBuilder.Build(
          command.AllowInventoryTransfers,
          command.AllowAdvancedReports,
          command.AllowAuditLogs),
        clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.InvalidPlanData);
    }

    await planRepository.UpdateAsync(plan, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SubscriptionPlanResponseMapper.ToResponse(plan));
  }
}

public sealed class ActivateSubscriptionPlanCommandHandler(
  ISubscriptionPlanRepository planRepository,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<SubscriptionPlanResponse>> Handle(
    ActivateSubscriptionPlanCommand command,
    CancellationToken cancellationToken = default)
  {
    var plan = await planRepository.GetByIdAsync(command.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.PlanNotFound);
    }

    plan.Activate(clock.UtcNow);
    await planRepository.UpdateAsync(plan, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SubscriptionPlanResponseMapper.ToResponse(plan));
  }
}

public sealed class DeactivateSubscriptionPlanCommandHandler(
  ISubscriptionPlanRepository planRepository,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<SubscriptionPlanResponse>> Handle(
    DeactivateSubscriptionPlanCommand command,
    CancellationToken cancellationToken = default)
  {
    var plan = await planRepository.GetByIdAsync(command.PlanId, cancellationToken);
    if (plan is null)
    {
      return Result.Failure<SubscriptionPlanResponse>(SubscriptionErrors.PlanNotFound);
    }

    plan.Deactivate(clock.UtcNow);
    await planRepository.UpdateAsync(plan, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SubscriptionPlanResponseMapper.ToResponse(plan));
  }
}

internal static class SubscriptionPlanFeatureBuilder
{
  internal static SubscriptionFeature Build(
    bool allowInventoryTransfers,
    bool allowAdvancedReports,
    bool allowAuditLogs)
  {
    var features = SubscriptionFeature.Sales |
      SubscriptionFeature.Products |
      SubscriptionFeature.Branches |
      SubscriptionFeature.Users |
      SubscriptionFeature.Purchases |
      SubscriptionFeature.Invoices |
      SubscriptionFeature.Payments;

    if (allowInventoryTransfers)
    {
      features |= SubscriptionFeature.InventoryTransfers;
    }

    if (allowAdvancedReports)
    {
      features |= SubscriptionFeature.Reports;
    }

    if (allowAuditLogs)
    {
      features |= SubscriptionFeature.AuditLogs;
    }

    return features;
  }
}
