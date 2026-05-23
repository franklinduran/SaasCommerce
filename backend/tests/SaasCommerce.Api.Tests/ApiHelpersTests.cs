#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Api.Tests;

public sealed class ApiHelpersTests
{
  // ── ToPublicErrorCode ────────────────────────────────────────────────────

  [Theory]
  [InlineData("validation_error", "VALIDATION_ERROR")]
  [InlineData("catalog.invalid_product", "VALIDATION_ERROR")]
  [InlineData("inventory.invalid_adjustment", "VALIDATION_ERROR")]
  [InlineData("customers.invalid_customer", "VALIDATION_ERROR")]
  [InlineData("credits.invalid_operation", "VALIDATION_ERROR")]
  [InlineData("credits.payment_exceeds_balance", "VALIDATION_ERROR")]
  [InlineData("credits.credit_blocked", "VALIDATION_ERROR")]
  [InlineData("credits.credit_limit_exceeded", "VALIDATION_ERROR")]
  [InlineData("sales.invalid_sale", "VALIDATION_ERROR")]
  [InlineData("suppliers.invalid_supplier", "VALIDATION_ERROR")]
  [InlineData("purchases.invalid_purchase", "VALIDATION_ERROR")]
  [InlineData("purchases.invalid_state", "VALIDATION_ERROR")]
  [InlineData("invoices.invalid_invoice", "VALIDATION_ERROR")]
  [InlineData("invoices.invalid_state", "VALIDATION_ERROR")]
  [InlineData("sales.invalid_state", "VALIDATION_ERROR")]
  public void ToPublicErrorCode_ShouldReturnValidationError_ForValidationCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("identity.invalid_credentials", "UNAUTHORIZED")]
  [InlineData("identity.invalid_refresh_token", "UNAUTHORIZED")]
  [InlineData("identity.not_authenticated", "UNAUTHORIZED")]
  public void ToPublicErrorCode_ShouldReturnUnauthorized_ForAuthCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("forbidden", "FORBIDDEN")]
  [InlineData("identity.forbidden", "FORBIDDEN")]
  [InlineData("identity.user_different_business", "FORBIDDEN")]
  public void ToPublicErrorCode_ShouldReturnForbidden_ForForbiddenCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("identity.invalid_current_user", "TENANT_CONTEXT_MISSING")]
  [InlineData("identity.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("catalog.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("inventory.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("customers.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("credits.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("invoices.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("sales.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("suppliers.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("purchases.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("cash.user_context_required", "TENANT_CONTEXT_MISSING")]
  [InlineData("expenses.user_context_required", "TENANT_CONTEXT_MISSING")]
  public void ToPublicErrorCode_ShouldReturnTenantContextMissing_ForContextCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("identity.user_not_found", "NOT_FOUND")]
  [InlineData("tenancy.business_not_found", "NOT_FOUND")]
  [InlineData("tenancy.branch_not_found", "NOT_FOUND")]
  [InlineData("catalog.category_not_found", "NOT_FOUND")]
  [InlineData("customers.customer_not_found", "NOT_FOUND")]
  [InlineData("credits.customer_not_found", "NOT_FOUND")]
  [InlineData("credits.sale_not_found", "NOT_FOUND")]
  [InlineData("invoices.invoice_not_found", "NOT_FOUND")]
  [InlineData("invoices.sale_not_found", "NOT_FOUND")]
  [InlineData("sales.sale_not_found", "NOT_FOUND")]
  [InlineData("sales.customer_not_found", "NOT_FOUND")]
  [InlineData("suppliers.supplier_not_found", "NOT_FOUND")]
  [InlineData("purchases.purchase_not_found", "NOT_FOUND")]
  [InlineData("purchases.supplier_not_found", "NOT_FOUND")]
  [InlineData("cash.session_not_found", "NOT_FOUND")]
  [InlineData("expenses.expense_not_found", "NOT_FOUND")]
  [InlineData("expenses.category_not_found", "NOT_FOUND")]
  public void ToPublicErrorCode_ShouldReturnNotFound_ForNotFoundCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("identity.cannot_disable_self", "VALIDATION_ERROR")]
  [InlineData("identity.cannot_remove_last_owner", "VALIDATION_ERROR")]
  [InlineData("identity.invalid_role", "VALIDATION_ERROR")]
  public void ToPublicErrorCode_ShouldReturnValidationError_ForIdentityManagementCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("account.duplicate_email", "CONFLICT")]
  [InlineData("account.duplicate_identification", "CONFLICT")]
  [InlineData("tenancy.duplicate_identification", "CONFLICT")]
  [InlineData("catalog.duplicate_category", "CONFLICT")]
  [InlineData("expenses.duplicate_category_name", "CONFLICT")]
  public void ToPublicErrorCode_ShouldReturnConflict_ForDuplicateCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Theory]
  [InlineData("catalog.product_not_found", "PRODUCT_NOT_FOUND")]
  [InlineData("inventory.product_not_found", "PRODUCT_NOT_FOUND")]
  [InlineData("sales.product_not_found", "PRODUCT_NOT_FOUND")]
  [InlineData("purchases.product_not_found", "PRODUCT_NOT_FOUND")]
  public void ToPublicErrorCode_ShouldReturnProductNotFound_ForProductCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Fact]
  public void ToPublicErrorCode_ShouldReturnSkuAlreadyExists_ForDuplicateSku()
  {
    var result = ApiHelpers.ToPublicErrorCode("catalog.duplicate_sku");

    result.Should().Be("PRODUCT_SKU_ALREADY_EXISTS");
  }

  [Fact]
  public void ToPublicErrorCode_ShouldReturnBarcodeAlreadyExists_ForDuplicateBarcode()
  {
    var result = ApiHelpers.ToPublicErrorCode("catalog.duplicate_barcode");

    result.Should().Be("PRODUCT_BARCODE_ALREADY_EXISTS");
  }

  [Fact]
  public void ToPublicErrorCode_ShouldReturnInventoryStockInsufficient_ForNegativeStock()
  {
    var result = ApiHelpers.ToPublicErrorCode("inventory.negative_stock");

    result.Should().Be("INVENTORY_STOCK_INSUFFICIENT");
  }

  [Theory]
  [InlineData("inventory.product_does_not_track_inventory", "INVENTORY_PRODUCT_NOT_TRACKED")]
  [InlineData("purchases.product_does_not_track_inventory", "INVENTORY_PRODUCT_NOT_TRACKED")]
  public void ToPublicErrorCode_ShouldReturnInventoryProductNotTracked_ForNotTrackedCodes(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }

  [Fact]
  public void ToPublicErrorCode_ShouldReturnUpperCaseWithUnderscore_ForUnknownCode()
  {
    var result = ApiHelpers.ToPublicErrorCode("some.unknown_error");

    result.Should().Be("SOME_UNKNOWN_ERROR");
  }

  // ── ToFailureStatusCode ──────────────────────────────────────────────────

  [Theory]
  [InlineData("UNAUTHORIZED", 401)]
  [InlineData("TENANT_CONTEXT_MISSING", 401)]
  [InlineData("AUTH_USER_ID_MISSING", 401)]
  [InlineData("AUTH_BUSINESS_ID_MISSING", 401)]
  public void ToFailureStatusCode_ShouldReturn401_ForAuthCodes(string code, int expectedStatus)
  {
    var result = ApiHelpers.ToFailureStatusCode(code);

    result.Should().Be(expectedStatus);
  }

  [Fact]
  public void ToFailureStatusCode_ShouldReturn403_ForForbidden()
  {
    var result = ApiHelpers.ToFailureStatusCode("FORBIDDEN");

    result.Should().Be(403);
  }

  [Theory]
  [InlineData("NOT_FOUND", 404)]
  [InlineData("PRODUCT_NOT_FOUND", 404)]
  public void ToFailureStatusCode_ShouldReturn404_ForNotFoundCodes(string code, int expectedStatus)
  {
    var result = ApiHelpers.ToFailureStatusCode(code);

    result.Should().Be(expectedStatus);
  }

  [Theory]
  [InlineData("CONFLICT", 409)]
  [InlineData("PRODUCT_SKU_ALREADY_EXISTS", 409)]
  [InlineData("PRODUCT_BARCODE_ALREADY_EXISTS", 409)]
  [InlineData("INVENTORY_STOCK_INSUFFICIENT", 409)]
  [InlineData("INVENTORY_PRODUCT_NOT_TRACKED", 409)]
  public void ToFailureStatusCode_ShouldReturn409_ForConflictCodes(string code, int expectedStatus)
  {
    var result = ApiHelpers.ToFailureStatusCode(code);

    result.Should().Be(expectedStatus);
  }

  [Fact]
  public void ToFailureStatusCode_ShouldReturn400_ForUnknownCodes()
  {
    var result = ApiHelpers.ToFailureStatusCode("SOME_UNKNOWN_CODE");

    result.Should().Be(400);
  }

  // ── ToApiError ───────────────────────────────────────────────────────────

  [Fact]
  public void ToApiError_ShouldMapDomainErrorToApiError()
  {
    var domainError = new DomainError("catalog.invalid_product", "Product is invalid.");

    var apiError = ApiHelpers.ToApiError(domainError);

    apiError.Code.Should().Be("VALIDATION_ERROR");
    apiError.Message.Should().Be("Product is invalid.");
    apiError.ValidationErrors.Should().BeNullOrEmpty();
  }

  [Fact]
  public void ToApiError_ShouldIncludeValidationErrors_WhenDetailsExist()
  {
    var domainError = new DomainError(
      "validation_error",
      "Validation failed.",
      [new DomainError("field.required", "Field is required.")]);

    var apiError = ApiHelpers.ToApiError(domainError);

    apiError.ValidationErrors.Should().ContainSingle(e =>
      e.Field == "field.required" && e.Message == "Field is required.");
  }

  // ── Expense error code mappings ─────────────────────────────────────────

  [Theory]
  [InlineData("expenses.invalid_expense", "VALIDATION_ERROR")]
  [InlineData("expenses.invalid_amount", "VALIDATION_ERROR")]
  [InlineData("expenses.invalid_payment_method", "VALIDATION_ERROR")]
  [InlineData("expenses.invalid_status", "VALIDATION_ERROR")]
  [InlineData("expenses.already_paid", "VALIDATION_ERROR")]
  [InlineData("expenses.already_cancelled", "VALIDATION_ERROR")]
  [InlineData("expenses.cannot_cancel_paid", "VALIDATION_ERROR")]
  [InlineData("expenses.cash_session_required", "VALIDATION_ERROR")]
  public void ToPublicErrorCode_ShouldReturnValidationError_ForExpenseErrors(string code, string expected)
  {
    var result = ApiHelpers.ToPublicErrorCode(code);

    result.Should().Be(expected);
  }
}

#pragma warning restore CA1707
