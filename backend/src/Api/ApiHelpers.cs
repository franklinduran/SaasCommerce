using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Api;

internal static class ApiHelpers
{
  internal static IResult ToApiResult<T>(
    Result<T> result,
    ICorrelationIdProvider correlationIdProvider,
    int? failureStatusCode = null,
    int? successStatusCode = null)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(correlationIdProvider);

    var apiError = result.IsFailure
      ? ToApiError(result.Error)
      : null;

    return result.IsSuccess
      ? Results.Json(
        ApiResponse.Success(result.Value, correlationIdProvider.CorrelationId),
        statusCode: successStatusCode ?? StatusCodes.Status200OK)
      : Results.Json(
        ApiResponse.Failure<T>(apiError!, correlationIdProvider.CorrelationId),
        statusCode: failureStatusCode ?? ToFailureStatusCode(apiError!.Code));
  }

  internal static ApiError ToApiError(DomainError error)
  {
    var validationErrors = error.Details?
      .Select(detail => new ValidationError(detail.Code, detail.Message))
      .ToArray();

    return new(ToPublicErrorCode(error.Code), error.Message, ValidationErrors: validationErrors);
  }

  internal static string ToPublicErrorCode(string code)
    => code switch
    {
      "validation_error" or
        "catalog.invalid_product" or
        "inventory.invalid_adjustment" or
        "customers.invalid_customer" or
        "credits.invalid_operation" or
        "credits.payment_exceeds_balance" or
        "credits.credit_blocked" or
        "credits.credit_limit_exceeded" or
        "sales.invalid_sale" or
        "suppliers.invalid_supplier" or
        "purchases.invalid_purchase" or
        "purchases.invalid_state" or
        "invoices.invalid_invoice" or
        "invoices.invalid_state" or
        "sales.invalid_state" or
        "subscription.invalid_plan_data" => ApiErrorCodes.ValidationError,
      "identity.invalid_credentials" or
        "identity.invalid_refresh_token" or
        "identity.not_authenticated" => ApiErrorCodes.Unauthorized,
      "forbidden" or
        "identity.forbidden" or
        "identity.user_different_business" => ApiErrorCodes.Forbidden,
      "identity.invalid_current_user" or
        "identity.user_context_required" or
        "catalog.user_context_required" or
        "inventory.user_context_required" or
        "customers.user_context_required" or
        "credits.user_context_required" or
        "invoices.user_context_required" or
        "sales.user_context_required" or
        "subscription.user_context_required" or
        "suppliers.user_context_required" or
        "purchases.user_context_required" or
        "cash.user_context_required" => ApiErrorCodes.TenantContextMissing,
      "identity.user_not_found" or
        "tenancy.business_not_found" or
        "tenancy.branch_not_found" or
        "catalog.category_not_found" or
        "customers.customer_not_found" or
        "credits.customer_not_found" or
        "credits.sale_not_found" or
        "invoices.invoice_not_found" or
        "invoices.sale_not_found" or
        "sales.sale_not_found" or
        "sales.customer_not_found" or
        "suppliers.supplier_not_found" or
        "purchases.purchase_not_found" or
        "purchases.supplier_not_found" or
        "subscription.plan_not_found" or
        "subscription.not_found" or
        "cash.session_not_found" => ApiErrorCodes.NotFound,
      "identity.cannot_disable_self" or
        "identity.cannot_remove_last_owner" or
        "identity.invalid_role" or
        "identity.password_too_short" or
        "identity.invalid_email" or
        "identity.cannot_create_owner_role" or
        "identity.cannot_reset_own_password" or
        "identity.user_already_active" or
        "identity.user_already_inactive" => ApiErrorCodes.ValidationError,
      "identity.duplicate_email" or
        "account.duplicate_email" or
        "account.duplicate_identification" or
        "tenancy.duplicate_identification" or
        "catalog.duplicate_category" or
        "subscription.duplicate" or
        "subscription.duplicate_plan_code" => ApiErrorCodes.Conflict,
      "subscription.expired" or
        "subscription.suspended" or
        "subscription.cancelled" or
        "subscription.feature_not_available" or
        "subscription.plan_not_active" => ApiErrorCodes.Forbidden,
      "catalog.product_not_found" or
        "inventory.product_not_found" or
        "sales.product_not_found" or
        "purchases.product_not_found" => ApiErrorCodes.ProductNotFound,
      "catalog.duplicate_sku" => ApiErrorCodes.ProductSkuAlreadyExists,
      "catalog.duplicate_barcode" => ApiErrorCodes.ProductBarcodeAlreadyExists,
      "inventory.negative_stock" => ApiErrorCodes.InventoryStockInsufficient,
      "inventory.product_does_not_track_inventory" or
        "purchases.product_does_not_track_inventory" => ApiErrorCodes.InventoryProductNotTracked,
      _ => code.ToUpperInvariant().Replace('.', '_')
    };

  internal static int ToFailureStatusCode(string publicErrorCode)
    => publicErrorCode switch
    {
      ApiErrorCodes.Unauthorized or
        ApiErrorCodes.TenantContextMissing or
        ApiErrorCodes.AuthUserIdMissing or
        ApiErrorCodes.AuthBusinessIdMissing => StatusCodes.Status401Unauthorized,
      ApiErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
      ApiErrorCodes.NotFound or
        ApiErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
        ApiErrorCodes.Conflict or
        "SUBSCRIPTION_LIMIT_REACHED" or
        ApiErrorCodes.ProductSkuAlreadyExists or
        ApiErrorCodes.ProductBarcodeAlreadyExists or
        ApiErrorCodes.InventoryStockInsufficient or
        ApiErrorCodes.InventoryProductNotTracked => StatusCodes.Status409Conflict,
      _ => StatusCodes.Status400BadRequest
    };
}
