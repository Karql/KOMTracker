using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KomTracker.Infrastructure.Persistence.Migrations;

public partial class AddSegmentAndSegmentEffortTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "segment",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false),
                name = table.Column<string>(type: "text", nullable: true),
                activity_type = table.Column<string>(type: "text", nullable: true),
                distance = table.Column<float>(type: "real", nullable: false),
                average_grade = table.Column<float>(type: "real", nullable: false),
                maximum_grade = table.Column<float>(type: "real", nullable: false),
                elevation_high = table.Column<float>(type: "real", nullable: false),
                elevation_low = table.Column<float>(type: "real", nullable: false),
                start_latitude = table.Column<float>(type: "real", nullable: false),
                start_longitude = table.Column<float>(type: "real", nullable: false),
                end_latitude = table.Column<float>(type: "real", nullable: false),
                end_longitude = table.Column<float>(type: "real", nullable: false),
                climb_category = table.Column<int>(type: "integer", nullable: false),
                city = table.Column<string>(type: "text", nullable: true),
                country = table.Column<string>(type: "text", nullable: true),
                @private = table.Column<bool>(name: "private", type: "boolean", nullable: false),
                hazardous = table.Column<bool>(type: "boolean", nullable: false),
                starred = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_segment", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "segment_effort",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false),
                activity_id = table.Column<long>(type: "bigint", nullable: false),
                athlete_id = table.Column<int>(type: "integer", nullable: false),
                segment_id = table.Column<long>(type: "bigint", nullable: false),
                name = table.Column<string>(type: "text", nullable: true),
                elapsed_time = table.Column<int>(type: "integer", nullable: false),
                moving_time = table.Column<int>(type: "integer", nullable: false),
                start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                start_date_local = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                distance = table.Column<float>(type: "real", nullable: false),
                start_index = table.Column<int>(type: "integer", nullable: false),
                end_index = table.Column<int>(type: "integer", nullable: false),
                average_cadence = table.Column<float>(type: "real", nullable: false),
                device_watts = table.Column<bool>(type: "boolean", nullable: false),
                average_watts = table.Column<float>(type: "real", nullable: false),
                average_heartrate = table.Column<float>(type: "real", nullable: false),
                max_heartrate = table.Column<float>(type: "real", nullable: false),
                pr_rank = table.Column<int>(type: "integer", nullable: true),
                kom_rank = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_segment_effort", x => x.id);
                table.ForeignKey(
                    name: "FK_segment_effort_athlete_athlete_id",
                    column: x => x.athlete_id,
                    principalTable: "athlete",
                    principalColumn: "athlete_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_segment_effort_segment_segment_id",
                    column: x => x.segment_id,
                    principalTable: "segment",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_segment_effort_athlete_id",
            table: "segment_effort",
            column: "athlete_id");

        migrationBuilder.CreateIndex(
            name: "IX_segment_effort_segment_id",
            table: "segment_effort",
            column: "segment_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "segment_effort");

        migrationBuilder.DropTable(
            name: "segment");
    }
}
