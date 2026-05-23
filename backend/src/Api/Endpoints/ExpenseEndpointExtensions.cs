using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Sales.Application.Expenses;
using SaasCommerce.Modules.Sales.Contracts.Requests;

namespace SaasCommerce.Api.Endpoints;

internal static class ExpenseEndpointExtensions
{
  private const string ExpenseTag = "Expenses";
  private const string CategoryTag = "ExpenseCategories";

  internal static WebApplication MapExpenseEndpoints(this WebApplication app)
  {
    // ── Expense Categories ───────────────────────────────────────────────────

    app.MapGet(
      "/api/expense-categories",
      async (
        GetExpenseCategoriesHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesView}")
      .WithTags(CategoryTag);

    app.MapPost(
      "/api/expense-categories",
      async (
        CreateExpenseCategoryRequest request,
        CreateExpenseCategoryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateExpenseCategoryCommand(request.Name),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpenseCategoriesManage}")
      .WithTags(CategoryTag);

    // ── Operating Expenses ───────────────────────────────────────────────────

    app.MapGet(
      "/api/expenses",
      async (
        Guid? branchId,
        Guid? categoryId,
        string? status,
        string? paymentMethod,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        int? page,
        int? pageSize,
        GetOperatingExpensesHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetOperatingExpensesQuery(
            branchId, categoryId, status, paymentMethod,
            dateFrom, dateTo, page ?? 1, pageSize ?? 20),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesView}")
      .WithTags(ExpenseTag);

    app.MapGet(
      "/api/expenses/summary",
      async (
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid? branchId,
        GetExpenseSummaryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetExpenseSummaryQuery(dateFrom, dateTo, branchId),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesView}")
      .WithTags(ExpenseTag);

    app.MapGet(
      "/api/expenses/{id:guid}",
      async (
        Guid id,
        GetOperatingExpenseDetailHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetOperatingExpenseDetailQuery(id), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesView}")
      .WithTags(ExpenseTag);

    app.MapPost(
      "/api/expenses",
      async (
        CreateOperatingExpenseRequest request,
        CreateOperatingExpenseHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreateOperatingExpenseCommand(
            request.BranchId,
            request.CategoryId,
            request.Description,
            request.Amount,
            request.PaymentMethod,
            request.Status,
            request.ExpenseDate,
            request.Notes),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider, successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesCreate}")
      .WithTags(ExpenseTag);

    app.MapPost(
      "/api/expenses/{id:guid}/pay",
      async (
        Guid id,
        PayOperatingExpenseRequest request,
        PayOperatingExpenseHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new PayOperatingExpenseCommand(id, request.PaymentMethod),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesManage}")
      .WithTags(ExpenseTag);

    app.MapPost(
      "/api/expenses/{id:guid}/cancel",
      async (
        Guid id,
        CancelOperatingExpenseHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CancelOperatingExpenseCommand(id),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ExpensesManage}")
      .WithTags(ExpenseTag);

    return app;
  }
}
