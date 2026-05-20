using SaasCommerce.Api;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Settings.Application;

namespace SaasCommerce.Api.Endpoints;

internal static class SettingsEndpointExtensions
{
  private const string SettingsTag = "Settings";

  internal static WebApplication MapSettingsEndpoints(this WebApplication app)
  {
    // ── Business Settings ─────────────────────────────────────────────────
    app.MapGet(
      "/api/settings/business",
      async (
        GetBusinessSettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetBusinessSettingsQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsView}")
      .WithTags(SettingsTag);

    app.MapPut(
      "/api/settings/business",
      async (
        UpdateBusinessSettingsRequest request,
        UpdateBusinessSettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateBusinessSettingsCommand(
            request.CommercialName, request.LegalName, request.Rnc,
            request.Phone, request.Email, request.Address,
            request.Currency, request.Timezone,
            request.LogoUrl, request.ReceiptFooterText),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsUpdate}")
      .WithTags(SettingsTag);

    // ── Sales Settings ────────────────────────────────────────────────────
    app.MapGet(
      "/api/settings/sales",
      async (
        GetSalesSettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetSalesSettingsQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsView}")
      .WithTags(SettingsTag);

    app.MapPut(
      "/api/settings/sales",
      async (
        UpdateSalesSettingsRequest request,
        UpdateSalesSettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateSalesSettingsCommand(
            request.AllowNegativeStock, request.AllowDiscounts,
            request.RequireCustomerForCreditSale, request.DefaultPaymentMethod,
            request.EnableReceiptPrintAfterSale, request.EnableInvoiceAutoGeneration),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsUpdate}")
      .WithTags(SettingsTag);

    // ── Inventory Settings ────────────────────────────────────────────────
    app.MapGet(
      "/api/settings/inventory",
      async (
        GetInventorySettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetInventorySettingsQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsView}")
      .WithTags(SettingsTag);

    app.MapPut(
      "/api/settings/inventory",
      async (
        UpdateInventorySettingsRequest request,
        UpdateInventorySettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateInventorySettingsCommand(
            request.EnableLowStockAlerts, request.DefaultLowStockThreshold,
            request.RequireReasonForInventoryAdjustment,
            request.AllowInventoryTransferBetweenBranches),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsUpdate}")
      .WithTags(SettingsTag);

    // ── Billing Settings ──────────────────────────────────────────────────
    app.MapGet(
      "/api/settings/billing",
      async (
        GetBillingSettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetBillingSettingsQuery(), cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsView}")
      .WithTags(SettingsTag);

    app.MapPut(
      "/api/settings/billing",
      async (
        UpdateBillingSettingsRequest request,
        UpdateBillingSettingsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new UpdateBillingSettingsCommand(
            request.ReceiptHeaderText, request.ReceiptFooterText,
            request.ShowLogoOnReceipt, request.ShowRncOnReceipt,
            request.EnableInvoiceAutoGeneration, request.InvoicePrefix,
            request.InvoiceSequenceStart),
          cancellationToken);
        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SettingsUpdate}")
      .WithTags(SettingsTag);

    return app;
  }
}
