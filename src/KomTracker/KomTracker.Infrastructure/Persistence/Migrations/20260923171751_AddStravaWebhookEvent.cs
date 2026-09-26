using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStravaWebhookEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "webhook_event",
                schema: "strava",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    object_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    object_id = table.Column<long>(type: "bigint", nullable: false),
                    aspect_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updates = table.Column<string>(type: "jsonb", nullable: true),
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    subscription_id = table.Column<int>(type: "integer", nullable: false),
                    event_time = table.Column<long>(type: "bigint", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_event", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_event_event_time",
                schema: "strava",
                table: "webhook_event",
                column: "event_time");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_event_processed",
                schema: "strava",
                table: "webhook_event",
                column: "processed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_event",
                schema: "strava");
        }
    }
}
