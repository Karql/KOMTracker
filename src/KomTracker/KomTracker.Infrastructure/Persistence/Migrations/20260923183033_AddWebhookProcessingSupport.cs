using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookProcessingSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhook_event_processed",
                schema: "strava",
                table: "webhook_event");

            migrationBuilder.AddColumn<long>(
                name: "activity_id",
                schema: "strava",
                table: "activity_sync_history",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "strava",
                table: "activity_sync_history",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Job");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_event_processed",
                schema: "strava",
                table: "webhook_event",
                column: "processed",
                filter: "processed = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_webhook_event_processed",
                schema: "strava",
                table: "webhook_event");

            migrationBuilder.DropColumn(
                name: "activity_id",
                schema: "strava",
                table: "activity_sync_history");

            migrationBuilder.DropColumn(
                name: "type",
                schema: "strava",
                table: "activity_sync_history");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_event_processed",
                schema: "strava",
                table: "webhook_event",
                column: "processed");
        }
    }
}
