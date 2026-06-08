using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Storage;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Identity.Application.PilotBusiness;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.SharedKernel.Tenancy;
using SaasCommerce.Api.Infrastructure.Storage;

namespace SaasCommerce.Api.Endpoints;

internal static class AdminEndpointExtensions
{
  private const string AdminTag = "Admin";

  internal static WebApplication MapAdminEndpoints(this WebApplication app)
  {
    app.MapPost(
      "/api/admin/pilot-businesses",
      async (
        CreatePilotBusinessRequest request,
        CreatePilotBusinessHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new CreatePilotBusinessCommand(
            request.BusinessName,
            request.IdentificationType,
            request.IdentificationNumber,
            request.Phone,
            request.BranchName,
            request.AdminFullName,
            request.AdminEmail,
            request.AdminPassword),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider,
          successStatusCode: StatusCodes.Status201Created);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SaasManageBusinesses}")
      .WithTags(AdminTag);

    app.MapGet(
      "/api/admin/pilot-metrics",
      async (
        GetPilotMetricsHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetPilotMetricsQuery(), cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SaasPilotMetrics}")
      .WithTags(AdminTag);

    // ── Seed product images (dev only) ────────────────────────────────────────
    app.MapPost(
      "/api/admin/seed-product-images",
      async (
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory,
        IStorageService storageService,
        MinioOptions minioOptions,
        AppDbContext dbContext,
        ICurrentUserService currentUser,
        IClock clock,
        IUnitOfWork unitOfWork,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        if (!env.IsDevelopment())
        {
          return Results.NotFound();
        }

        if (currentUser.BusinessId is not Guid businessId)
        {
          return Results.Unauthorized();
        }

        var products = await dbContext.Set<Product>()
          .Where(p => p.BusinessId == new BusinessId(businessId) && p.ImageUrl == null)
          .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
          return Results.Ok(new { updated = 0, message = "Todos los productos ya tienen imagen." });
        }

        var httpClient = httpClientFactory.CreateClient("picsum");
        var updated = 0;
        var now = clock.UtcNow;

        foreach (var product in products)
        {
          try
          {
            // picsum.photos/seed/{seed}/{w}/{h} → always the same image for a given seed
            var url = $"https://picsum.photos/seed/{Uri.EscapeDataString(product.Sku)}/400/300";
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) continue;

            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            using var stream = new MemoryStream(imageBytes);

            var objectName = $"{businessId}/{product.Id}.jpg";
            var imageUrl = await storageService.UploadAsync(
              minioOptions.BucketName,
              objectName,
              stream,
              "image/jpeg",
              imageBytes.Length,
              cancellationToken);

            product.SetImageUrl(imageUrl, now);
            updated++;
          }
          catch
          {
            // Skip products that fail — partial success is fine for seeding
          }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { updated, total = products.Count });
      })
      .RequireAuthorization()
      .WithTags(AdminTag);

    return app;
  }
}
