using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Catalog.Application.Import;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Api.Endpoints;

internal static class ProductImportEndpointExtensions
{
  private const string CatalogTag = "Catalog";

  private const string CsvTemplate =
    "Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity\r\n" +
    "Arroz El Gallo 5lbs,ARR-GALL-5LB,Víveres,175.00,140.00,100\r\n" +
    "Cerveza Presidente 12oz,CRV-PRES-12,Bebidas,65.00,47.00,120\r\n" +
    "Leche Parmalat 1L,LEC-PARM-1L,Lácteos,95.00,75.00,60\r\n";

  internal static WebApplication MapProductImportEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/products/import/template",
      (ICorrelationIdProvider correlationIdProvider) =>
      {
        return Results.Content(
          CsvTemplate,
          contentType: "text/csv",
          contentEncoding: System.Text.Encoding.UTF8);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProductsImport}")
      .WithTags(CatalogTag);

    app.MapPost(
      "/api/products/import",
      async (
        IFormFile file,
        ImportProductsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        if (file is null || file.Length == 0)
        {
          var emptyResult = Result.Failure<Modules.Catalog.Contracts.Responses.ImportProductsResponse>(
            ImportProductsErrors.EmptyFile);

          return ApiHelpers.ToApiResult(emptyResult, correlationIdProvider);
        }

        await using var stream = file.OpenReadStream();
        var result = await handler.Handle(
          new ImportProductsCommand(stream, CreateInitialInventory: true),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider,
          successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProductsImport}")
      .DisableAntiforgery()
      .WithTags(CatalogTag);

    return app;
  }
}
