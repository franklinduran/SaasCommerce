using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202605150001_CatalogInventory")]
public partial class CatalogInventory : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.EnsureSchema(name: "catalog");
    migrationBuilder.EnsureSchema(name: "inventory");

    migrationBuilder.CreateTable(
      name: "categories",
      schema: "catalog",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
        IsActive = table.Column<bool>(type: "boolean", nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_categories", category => category.Id);
      });

    migrationBuilder.CreateTable(
      name: "products",
      schema: "catalog",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        ProductType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
        CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
        BrandId = table.Column<Guid>(type: "uuid", nullable: true),
        ParentProductId = table.Column<Guid>(type: "uuid", nullable: true),
        Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
        Description = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
        Sku = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
        Barcode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
        SearchName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
        InternalCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
        SupplierCode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
        UnitOfMeasure = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
        SalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
        CostPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
        WholesalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
        MinSalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
        TaxCategory = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
        TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
        IsTaxIncluded = table.Column<bool>(type: "boolean", nullable: false),
        AllowsDiscount = table.Column<bool>(type: "boolean", nullable: false),
        TrackInventory = table.Column<bool>(type: "boolean", nullable: false),
        MinimumStock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
        MaximumStock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
        ReorderPoint = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
        AllowNegativeStock = table.Column<bool>(type: "boolean", nullable: false),
        VariantName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
        AttributesJson = table.Column<string>(type: "jsonb", nullable: true),
        IsActive = table.Column<bool>(type: "boolean", nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_products", product => product.Id);
      });

    migrationBuilder.CreateTable(
      name: "product_components",
      schema: "catalog",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        ProductId = table.Column<Guid>(type: "uuid", nullable: false),
        ComponentProductId = table.Column<Guid>(type: "uuid", nullable: false),
        Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_product_components", component => component.Id);
      });

    migrationBuilder.CreateTable(
      name: "stock_items",
      schema: "inventory",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        ProductId = table.Column<Guid>(type: "uuid", nullable: false),
        Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_stock_items", stockItem => stockItem.Id);
      });

    migrationBuilder.CreateTable(
      name: "inventory_movements",
      schema: "inventory",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        ProductId = table.Column<Guid>(type: "uuid", nullable: false),
        PreviousStock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
        NewStock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
        Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
        Reason = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
        UserId = table.Column<Guid>(type: "uuid", nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_inventory_movements", movement => movement.Id);
      });

    migrationBuilder.CreateIndex(
      name: "ix_categories_business_id_name",
      schema: "catalog",
      table: "categories",
      columns: ["BusinessId", "Name"],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_inventory_movements_business_id_product_id_created_at",
      schema: "inventory",
      table: "inventory_movements",
      columns: ["BusinessId", "ProductId", "CreatedAt"]);

    migrationBuilder.CreateIndex(
      name: "ix_product_components_business_id_product_id_component_product_id",
      schema: "catalog",
      table: "product_components",
      columns: ["BusinessId", "ProductId", "ComponentProductId"],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_products_business_id_barcode",
      schema: "catalog",
      table: "products",
      columns: ["BusinessId", "Barcode"],
      unique: true,
      filter: "\"Barcode\" IS NOT NULL");

    migrationBuilder.CreateIndex(
      name: "ix_products_business_id_sku",
      schema: "catalog",
      table: "products",
      columns: ["BusinessId", "Sku"],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_products_business_id_search_name",
      schema: "catalog",
      table: "products",
      columns: ["BusinessId", "SearchName"]);

    migrationBuilder.CreateIndex(
      name: "ix_stock_items_business_id_product_id",
      schema: "inventory",
      table: "stock_items",
      columns: ["BusinessId", "ProductId"],
      unique: true);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(name: "categories", schema: "catalog");
    migrationBuilder.DropTable(name: "inventory_movements", schema: "inventory");
    migrationBuilder.DropTable(name: "product_components", schema: "catalog");
    migrationBuilder.DropTable(name: "products", schema: "catalog");
    migrationBuilder.DropTable(name: "stock_items", schema: "inventory");
  }
}
