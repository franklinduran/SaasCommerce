using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202605270002_AddBetaFeedback")]
public partial class AddBetaFeedback : Migration
{
  private const string FeedbackSchema = "feedback";
  private const string BetaFeedbackTable = "beta_feedback";
  private static readonly string[] BusinessCategoryColumns = ["BusinessId", "Category"];
  private static readonly string[] BusinessStatusCreatedAtColumns = ["BusinessId", "Status", "CreatedAt"];
  private static readonly string[] BusinessUserColumns = ["BusinessId", "UserId"];

  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.EnsureSchema(name: FeedbackSchema);

    migrationBuilder.CreateTable(
      name: BetaFeedbackTable,
      schema: FeedbackSchema,
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        UserId = table.Column<Guid>(type: "uuid", nullable: false),
        Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
        Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
        Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
        Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
        ContextUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
        ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_beta_feedback", x => x.Id);
      });

    migrationBuilder.CreateIndex(
      name: "IX_beta_feedback_BusinessId_Category",
      schema: FeedbackSchema,
      table: BetaFeedbackTable,
      columns: BusinessCategoryColumns);

    migrationBuilder.CreateIndex(
      name: "IX_beta_feedback_BusinessId_Status_CreatedAt",
      schema: FeedbackSchema,
      table: BetaFeedbackTable,
      columns: BusinessStatusCreatedAtColumns);

    migrationBuilder.CreateIndex(
      name: "IX_beta_feedback_BusinessId_UserId",
      schema: FeedbackSchema,
      table: BetaFeedbackTable,
      columns: BusinessUserColumns);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: BetaFeedbackTable,
      schema: FeedbackSchema);
  }
}
