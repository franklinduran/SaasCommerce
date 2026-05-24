namespace SaasCommerce.Api;

internal static class ApiErrorCodes
{
  public const string ValidationError = "VALIDATION_ERROR";
  public const string Unauthorized = "UNAUTHORIZED";
  public const string Forbidden = "FORBIDDEN";
  public const string NotFound = "NOT_FOUND";
  public const string Conflict = "CONFLICT";
  public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
  public const string TenantContextMissing = "TENANT_CONTEXT_MISSING";
  public const string AuthUserIdMissing = "AUTH_USER_ID_MISSING";
  public const string AuthBusinessIdMissing = "AUTH_BUSINESS_ID_MISSING";
  public const string ProductNotFound = "PRODUCT_NOT_FOUND";
  public const string ProductSkuAlreadyExists = "PRODUCT_SKU_ALREADY_EXISTS";
  public const string ProductBarcodeAlreadyExists = "PRODUCT_BARCODE_ALREADY_EXISTS";
  public const string InventoryStockInsufficient = "INVENTORY_STOCK_INSUFFICIENT";
  public const string InventoryProductNotTracked = "INVENTORY_PRODUCT_NOT_TRACKED";
  public const string TooManyRequests = "TOO_MANY_REQUESTS";
}
