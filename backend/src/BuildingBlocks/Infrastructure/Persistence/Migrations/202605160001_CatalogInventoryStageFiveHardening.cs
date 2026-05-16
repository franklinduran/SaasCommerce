using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202605160001_CatalogInventoryStageFiveHardening")]
public partial class CatalogInventoryStageFiveHardening : Migration
{
  private const string BranchIdColumn = "BranchId";
  private const string BusinessIdColumn = "BusinessId";
  private const string CatalogSchema = "catalog";
  private const string CategoriesTable = "categories";
  private const string InventorySchema = "inventory";
  private const string InventoryMovementsTable = "inventory_movements";
  private const string ProductIdColumn = "ProductId";
  private const string StockItemsTable = "stock_items";

  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
      name: "Description",
      schema: CatalogSchema,
      table: CategoriesTable,
      type: "character varying(300)",
      maxLength: 300,
      nullable: true);

    migrationBuilder.AddColumn<DateTimeOffset>(
      name: "UpdatedAt",
      schema: CatalogSchema,
      table: CategoriesTable,
      type: "timestamp with time zone",
      nullable: true);

    migrationBuilder.AddColumn<Guid>(
      name: BranchIdColumn,
      schema: InventorySchema,
      table: StockItemsTable,
      type: "uuid",
      nullable: false,
      defaultValue: Guid.Empty);

    migrationBuilder.AddColumn<Guid>(
      name: BranchIdColumn,
      schema: InventorySchema,
      table: InventoryMovementsTable,
      type: "uuid",
      nullable: false,
      defaultValue: Guid.Empty);

    migrationBuilder.DropIndex(
      name: "ix_stock_items_business_id_product_id",
      schema: InventorySchema,
      table: StockItemsTable);

    migrationBuilder.DropIndex(
      name: "ix_inventory_movements_business_id_product_id_created_at",
      schema: InventorySchema,
      table: InventoryMovementsTable);

    migrationBuilder.CreateIndex(
      name: "ix_stock_items_business_id_branch_id_product_id",
      schema: InventorySchema,
      table: StockItemsTable,
      columns: [BusinessIdColumn, BranchIdColumn, ProductIdColumn],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_inventory_movements_business_id_branch_id_product_id_created_at",
      schema: InventorySchema,
      table: InventoryMovementsTable,
      columns: [BusinessIdColumn, BranchIdColumn, ProductIdColumn, "CreatedAt"]);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
      name: "ix_stock_items_business_id_branch_id_product_id",
      schema: InventorySchema,
      table: StockItemsTable);

    migrationBuilder.DropIndex(
      name: "ix_inventory_movements_business_id_branch_id_product_id_created_at",
      schema: InventorySchema,
      table: InventoryMovementsTable);

    migrationBuilder.DropColumn(name: "Description", schema: CatalogSchema, table: CategoriesTable);
    migrationBuilder.DropColumn(name: "UpdatedAt", schema: CatalogSchema, table: CategoriesTable);
    migrationBuilder.DropColumn(name: BranchIdColumn, schema: InventorySchema, table: StockItemsTable);
    migrationBuilder.DropColumn(name: BranchIdColumn, schema: InventorySchema, table: InventoryMovementsTable);

    migrationBuilder.CreateIndex(
      name: "ix_stock_items_business_id_product_id",
      schema: InventorySchema,
      table: StockItemsTable,
      columns: [BusinessIdColumn, ProductIdColumn],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_inventory_movements_business_id_product_id_created_at",
      schema: InventorySchema,
      table: InventoryMovementsTable,
      columns: [BusinessIdColumn, ProductIdColumn, "CreatedAt"]);
  }
}
