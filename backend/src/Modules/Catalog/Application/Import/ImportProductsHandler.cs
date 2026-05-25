using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Import;

public sealed class ImportProductsHandler(
  ICatalogProductRepository products,
  ICatalogCategoryRepository categories,
  IInventoryRepository inventory,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  private const int MaxRows = 500;

  public Task<Result<ImportProductsResponse>> Handle(
    ImportProductsCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<ImportProductsResponse>> HandleCoreAsync(
    ImportProductsCommand command,
    CancellationToken cancellationToken)
  {
    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } rawBusinessId
      || currentUser.UserId == Guid.Empty)
    {
      return Result.Failure<ImportProductsResponse>(ImportProductsErrors.UserContextRequired);
    }

    if (!currentUser.BranchId.HasValue)
    {
      return Result.Failure<ImportProductsResponse>(ImportProductsErrors.UserContextRequired);
    }

    var businessId = new BusinessId(rawBusinessId);
    var branchId = new BranchId(currentUser.BranchId.Value);
    var adminUserId = currentUser.UserId!.Value;
    var now = clock.UtcNow;

    // Parse CSV
    var rows = await ParseCsvAsync(command.CsvStream, cancellationToken);

    if (rows.Count == 0)
    {
      return Result.Failure<ImportProductsResponse>(ImportProductsErrors.EmptyFile);
    }

    // Load existing categories for name-based lookup
    var allCategories = await categories.ListAsync(businessId, cancellationToken);
    var categoryByName = allCategories.ToDictionary(
      c => c.Name.Trim().ToUpperInvariant(),
      c => c.Id,
      StringComparer.OrdinalIgnoreCase);

    // Track newly created categories in this import session
    var newCategoryIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

    var importedCount = 0;
    var skippedCount = 0;
    var rowErrors = new List<ImportRowError>();

    foreach (var row in rows)
    {
      // Validate row
      var rowValidation = ValidateRow(row);

      if (rowValidation.Count > 0)
      {
        skippedCount++;
        rowErrors.Add(new ImportRowError(row.RowNumber, row.Name ?? string.Empty, rowValidation));

        continue;
      }

      // Check for duplicate SKU
      var sku = row.Sku!.Trim().ToUpperInvariant();
      var existing = await products.GetBySkuAsync(sku, businessId, cancellationToken);

      if (existing is not null)
      {
        skippedCount++;
        rowErrors.Add(new ImportRowError(
          row.RowNumber,
          row.Name!,
          [$"SKU '{sku}' already exists."]));

        continue;
      }

      // Resolve category
      Guid? categoryId = null;

      if (!string.IsNullOrWhiteSpace(row.CategoryName))
      {
        var normalizedCatName = row.CategoryName.Trim();
        var catKey = normalizedCatName.ToUpperInvariant();

        if (categoryByName.TryGetValue(catKey, out var existingCatId))
        {
          categoryId = existingCatId;
        }
        else if (newCategoryIds.TryGetValue(catKey, out var newCatId))
        {
          categoryId = newCatId;
        }
        else
        {
          // Create a new category on the fly
          var newCategory = new Category(Guid.NewGuid(), businessId, normalizedCatName, null, now);
          await categories.AddAsync(newCategory, cancellationToken);
          newCategoryIds[catKey] = newCategory.Id;
          categoryId = newCategory.Id;
        }
      }

      // Create product
      var product = new Product(
        new ProductCreationContext(Guid.NewGuid(), businessId, now),
        new ProductIdentity(ProductType.Simple, row.Name!, null, categoryId, null, UnitOfMeasure.Unit),
        new ProductCodes(sku, null, null, null),
        new ProductPricing(row.SalePrice!.Value, row.CostPrice!.Value, null, null, TaxCategory.Exempt, 0m, false),
        new ProductInventorySettings(true, null, null, null, false),
        new ProductOptions(true, null, null, null));

      await products.AddAsync(product, cancellationToken);

      // Create initial stock if quantity provided
      if (command.CreateInitialInventory && row.StockQuantity.HasValue && row.StockQuantity.Value > 0)
      {
        var stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, product.Id, now);
        var movement = stockItem.ApplyAdjustment(
          row.StockQuantity.Value,
          InventoryMovementReason.InitialStock,
          adminUserId,
          false,
          now);

        await inventory.AddStockItemAsync(stockItem, cancellationToken);
        await inventory.AddMovementAsync(movement, cancellationToken);
      }

      importedCount++;
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new ImportProductsResponse(
      importedCount,
      skippedCount,
      rows.Count,
      rowErrors));
  }

  private static List<string> ValidateRow(ProductImportRow row)
  {
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(row.Name))
    {
      errors.Add("Name is required.");
    }

    if (string.IsNullOrWhiteSpace(row.Sku))
    {
      errors.Add("Sku is required.");
    }

    if (!row.SalePrice.HasValue || row.SalePrice.Value < 0)
    {
      errors.Add("SalePrice must be >= 0.");
    }

    if (!row.CostPrice.HasValue || row.CostPrice.Value < 0)
    {
      errors.Add("CostPrice must be >= 0.");
    }

    if (row.StockQuantity.HasValue && row.StockQuantity.Value < 0)
    {
      errors.Add("StockQuantity must be >= 0.");
    }

    return errors;
  }

  private static async Task<List<ProductImportRow>> ParseCsvAsync(
    Stream stream,
    CancellationToken cancellationToken)
  {
    var rows = new List<ProductImportRow>();
    using var reader = new StreamReader(stream, leaveOpen: true);

    // Read header
    var header = await reader.ReadLineAsync(cancellationToken);

    if (string.IsNullOrWhiteSpace(header))
    {
      return rows;
    }

    // Verify expected columns (case-insensitive)
    var headers = header.Split(',').Select(h => h.Trim().ToUpperInvariant()).ToArray();
    int nameIdx = Array.IndexOf(headers, "NAME");
    int skuIdx = Array.IndexOf(headers, "SKU");
    int catIdx = Array.IndexOf(headers, "CATEGORYNAME");
    int salePriceIdx = Array.IndexOf(headers, "SALEPRICE");
    int costPriceIdx = Array.IndexOf(headers, "COSTPRICE");
    int stockIdx = Array.IndexOf(headers, "STOCKQUANTITY");

    if (nameIdx < 0 || skuIdx < 0 || salePriceIdx < 0 || costPriceIdx < 0)
    {
      return rows; // will trigger InvalidFormat on empty result with header present
    }

    var rowNumber = 1;
    string? line;

    while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
    {
      if (string.IsNullOrWhiteSpace(line))
      {
        continue;
      }

      if (rowNumber >= MaxRows)
      {
        break;
      }

      var cells = SplitCsvLine(line);
      rowNumber++;

      rows.Add(new ProductImportRow(
        rowNumber,
        GetCell(cells, nameIdx),
        GetCell(cells, skuIdx),
        GetCell(cells, catIdx),
        ParseDecimal(GetCell(cells, salePriceIdx)),
        ParseDecimal(GetCell(cells, costPriceIdx)),
        ParseDecimal(GetCell(cells, stockIdx))));
    }

    return rows;
  }

  private static string[] SplitCsvLine(string line)
  {
    // Simple CSV splitter — handles quoted fields
    var result = new List<string>();
    var inQuotes = false;
    var current = new System.Text.StringBuilder();

    foreach (var ch in line)
    {
      if (ch == '"')
      {
        inQuotes = !inQuotes;
      }
      else if (ch == ',' && !inQuotes)
      {
        result.Add(current.ToString().Trim());
        current.Clear();
      }
      else
      {
        current.Append(ch);
      }
    }

    result.Add(current.ToString().Trim());

    return [.. result];
  }

  private static string? GetCell(string[] cells, int index)
    => index >= 0 && index < cells.Length
      ? string.IsNullOrWhiteSpace(cells[index]) ? null : cells[index].Trim()
      : null;

  private static decimal? ParseDecimal(string? value)
    => decimal.TryParse(
      value,
      System.Globalization.NumberStyles.Number,
      System.Globalization.CultureInfo.InvariantCulture,
      out var result)
      ? result
      : null;
}
