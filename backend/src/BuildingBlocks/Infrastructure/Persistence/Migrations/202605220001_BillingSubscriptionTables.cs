using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence.Migrations;

    /// <inheritdoc />
    public partial class BillingSubscriptionTables : Migration
    {
        private const string BillingSchema = "billing";
        private const string BusinessSubscriptionsTable = "business_subscriptions";
        private const string CurrentUtcTimestamp = "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'";
        private const string IntegerColumn = "integer";
        private const string SubscriptionPlansTable = "subscription_plans";
        private const string TimestampWithTimeZone = "timestamp with time zone";

        private static readonly string[] SubscriptionPlansActiveCreatedColumns = ["is_active", "created_at"];
        private static readonly string[] BusinessSubscriptionsStatusPeriodColumns = ["status", "current_period_end"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create billing schema if it doesn't exist
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS billing;");

            // Create subscription_plans table
            migrationBuilder.CreateTable(
                name: SubscriptionPlansTable,
                schema: BillingSchema,
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, defaultValue: ""),
                    monthly_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    max_branches = table.Column<int>(type: IntegerColumn, nullable: false, defaultValue: 1),
                    max_users = table.Column<int>(type: IntegerColumn, nullable: false, defaultValue: 1),
                    max_products = table.Column<int>(type: IntegerColumn, nullable: false, defaultValue: 100),
                    max_sales_per_month = table.Column<int>(type: IntegerColumn, nullable: false, defaultValue: 1000),
                    features = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: false, defaultValueSql: CurrentUtcTimestamp),
                    updated_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: false, defaultValueSql: CurrentUtcTimestamp)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_plans", x => x.id);
                });

            // Create indexes on subscription_plans
            migrationBuilder.CreateIndex(
                name: "ix_subscription_plans_is_active",
                schema: BillingSchema,
                table: SubscriptionPlansTable,
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plans_code_unique",
                schema: BillingSchema,
                table: SubscriptionPlansTable,
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plans_created_at",
                schema: BillingSchema,
                table: SubscriptionPlansTable,
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plans_active_created",
                schema: BillingSchema,
                table: SubscriptionPlansTable,
                columns: SubscriptionPlansActiveCreatedColumns);

            // Create business_subscriptions table
            migrationBuilder.CreateTable(
                name: BusinessSubscriptionsTable,
                schema: BillingSchema,
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "Trial"),
                    started_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: false),
                    trial_ends_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: true),
                    current_period_start = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: true),
                    current_period_end = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: true),
                    cancelled_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: true),
                    suspended_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: false, defaultValueSql: CurrentUtcTimestamp),
                    updated_at = table.Column<DateTimeOffset>(type: TimestampWithTimeZone, nullable: false, defaultValueSql: CurrentUtcTimestamp)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_business_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_business_subscriptions_subscription_plans",
                        column: x => x.plan_id,
                        principalSchema: BillingSchema,
                        principalTable: SubscriptionPlansTable,
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes on business_subscriptions
            migrationBuilder.CreateIndex(
                name: "ix_business_subscriptions_business_id_unique",
                schema: BillingSchema,
                table: BusinessSubscriptionsTable,
                column: "business_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_business_subscriptions_status",
                schema: BillingSchema,
                table: BusinessSubscriptionsTable,
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_business_subscriptions_plan_id",
                schema: BillingSchema,
                table: BusinessSubscriptionsTable,
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_business_subscriptions_trial_ends_at",
                schema: BillingSchema,
                table: BusinessSubscriptionsTable,
                column: "trial_ends_at");

            migrationBuilder.CreateIndex(
                name: "ix_business_subscriptions_current_period_end",
                schema: BillingSchema,
                table: BusinessSubscriptionsTable,
                column: "current_period_end");

            migrationBuilder.CreateIndex(
                name: "ix_business_subscriptions_status_period",
                schema: BillingSchema,
                table: BusinessSubscriptionsTable,
                columns: BusinessSubscriptionsStatusPeriodColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: BusinessSubscriptionsTable,
                schema: BillingSchema);

            migrationBuilder.DropTable(
                name: SubscriptionPlansTable,
                schema: BillingSchema);
        }
    }
