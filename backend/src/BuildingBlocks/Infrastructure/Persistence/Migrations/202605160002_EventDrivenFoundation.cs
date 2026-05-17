using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202605160002_EventDrivenFoundation")]
public partial class EventDrivenFoundation : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "inbox_messages",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        EventId = table.Column<Guid>(type: "uuid", nullable: false),
        ConsumerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
        ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_inbox_messages", x => x.Id);
      });

    migrationBuilder.CreateTable(
      name: "outbox_messages",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        EventId = table.Column<Guid>(type: "uuid", nullable: false),
        CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        EventType = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
        Payload = table.Column<string>(type: "text", nullable: false),
        OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        Attempts = table.Column<int>(type: "integer", nullable: false),
        LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
        Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_outbox_messages", x => x.Id);
      });

    migrationBuilder.CreateTable(
      name: "sale_saga_states",
      columns: table => new
      {
        CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
        SaleId = table.Column<Guid>(type: "uuid", nullable: false),
        BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
        BranchId = table.Column<Guid>(type: "uuid", nullable: false),
        UserId = table.Column<Guid>(type: "uuid", nullable: false),
        CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey("pk_sale_saga_states", x => x.CorrelationId);
      });

    migrationBuilder.CreateIndex(
      name: "ix_inbox_messages_business_id_consumer_name",
      table: "inbox_messages",
      columns: ["BusinessId", "ConsumerName"]);

    migrationBuilder.CreateIndex(
      name: "ix_inbox_messages_event_id_consumer_name",
      table: "inbox_messages",
      columns: ["EventId", "ConsumerName"],
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_outbox_messages_business_id_status",
      table: "outbox_messages",
      columns: ["BusinessId", "Status"]);

    migrationBuilder.CreateIndex(
      name: "ix_outbox_messages_event_id",
      table: "outbox_messages",
      column: "EventId",
      unique: true);

    migrationBuilder.CreateIndex(
      name: "ix_outbox_messages_status_occurred_at",
      table: "outbox_messages",
      columns: ["Status", "OccurredAt"]);

    migrationBuilder.CreateIndex(
      name: "ix_sale_saga_states_business_id_current_state",
      table: "sale_saga_states",
      columns: ["BusinessId", "CurrentState"]);

    migrationBuilder.CreateIndex(
      name: "ix_sale_saga_states_sale_id",
      table: "sale_saga_states",
      column: "SaleId",
      unique: true);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(name: "inbox_messages");
    migrationBuilder.DropTable(name: "outbox_messages");
    migrationBuilder.DropTable(name: "sale_saga_states");
  }
}
