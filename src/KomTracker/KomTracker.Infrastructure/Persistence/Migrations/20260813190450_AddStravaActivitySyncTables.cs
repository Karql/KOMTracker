using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStravaActivitySyncTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "strava");

            migrationBuilder.CreateTable(
                name: "activity",
                schema: "strava",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    gear_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    external_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    upload_id = table.Column<long>(type: "bigint", nullable: true),
                    distance = table.Column<double>(type: "double precision", nullable: false),
                    moving_time = table.Column<int>(type: "integer", nullable: false),
                    elapsed_time = table.Column<int>(type: "integer", nullable: false),
                    total_elevation_gain = table.Column<double>(type: "double precision", nullable: false),
                    elev_high = table.Column<double>(type: "double precision", nullable: true),
                    elev_low = table.Column<double>(type: "double precision", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    sport_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workout_type = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    utc_offset = table.Column<double>(type: "double precision", nullable: false),
                    trainer = table.Column<bool>(type: "boolean", nullable: false),
                    commute = table.Column<bool>(type: "boolean", nullable: false),
                    manual = table.Column<bool>(type: "boolean", nullable: false),
                    @private = table.Column<bool>(name: "private", type: "boolean", nullable: false),
                    flagged = table.Column<bool>(type: "boolean", nullable: false),
                    visibility = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    average_speed = table.Column<double>(type: "double precision", nullable: false),
                    max_speed = table.Column<double>(type: "double precision", nullable: false),
                    average_cadence = table.Column<double>(type: "double precision", nullable: true),
                    average_temp = table.Column<int>(type: "integer", nullable: true),
                    average_watts = table.Column<double>(type: "double precision", nullable: true),
                    weighted_average_watts = table.Column<double>(type: "double precision", nullable: true),
                    max_watts = table.Column<double>(type: "double precision", nullable: true),
                    device_watts = table.Column<bool>(type: "boolean", nullable: false),
                    kilojoules = table.Column<double>(type: "double precision", nullable: true),
                    has_heartrate = table.Column<bool>(type: "boolean", nullable: false),
                    average_heartrate = table.Column<double>(type: "double precision", nullable: true),
                    max_heartrate = table.Column<double>(type: "double precision", nullable: true),
                    suffer_score = table.Column<double>(type: "double precision", nullable: true),
                    achievement_count = table.Column<int>(type: "integer", nullable: false),
                    kudos_count = table.Column<int>(type: "integer", nullable: false),
                    comment_count = table.Column<int>(type: "integer", nullable: false),
                    athlete_count = table.Column<int>(type: "integer", nullable: false),
                    photo_count = table.Column<int>(type: "integer", nullable: false),
                    total_photo_count = table.Column<int>(type: "integer", nullable: false),
                    pr_count = table.Column<int>(type: "integer", nullable: false),
                    summary_polyline = table.Column<string>(type: "text", nullable: true),
                    start_lat = table.Column<double>(type: "double precision", nullable: true),
                    start_lng = table.Column<double>(type: "double precision", nullable: true),
                    end_lat = table.Column<double>(type: "double precision", nullable: true),
                    end_lng = table.Column<double>(type: "double precision", nullable: true),
                    device_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_athlete_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athlete",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_sync_history",
                schema: "strava",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    sync_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    upserted_count = table.Column<int>(type: "integer", nullable: false),
                    deleted_count = table.Column<int>(type: "integer", nullable: false),
                    activities_count = table.Column<int>(type: "integer", nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_sync_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_sync_history_athlete_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athlete",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "athlete_sync",
                schema: "strava",
                columns: table => new
                {
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    activities_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_athlete_sync", x => x.athlete_id);
                    table.ForeignKey(
                        name: "FK_athlete_sync_athlete_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athlete",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_athlete_id",
                schema: "strava",
                table: "activity",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_gear_id",
                schema: "strava",
                table: "activity",
                column: "gear_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_sync_history_athlete_id",
                schema: "strava",
                table: "activity_sync_history",
                column: "athlete_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity",
                schema: "strava");

            migrationBuilder.DropTable(
                name: "activity_sync_history",
                schema: "strava");

            migrationBuilder.DropTable(
                name: "athlete_sync",
                schema: "strava");
        }
    }
}
