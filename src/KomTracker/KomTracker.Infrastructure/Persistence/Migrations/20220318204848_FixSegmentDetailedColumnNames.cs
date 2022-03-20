using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class FixSegmentDetailedColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalElevationGain",
                table: "segment",
                newName: "total_elevation_gain");

            migrationBuilder.RenameColumn(
                name: "StarCount",
                table: "segment",
                newName: "star_count");

            migrationBuilder.RenameColumn(
                name: "EffortCount",
                table: "segment",
                newName: "effort_count");

            migrationBuilder.RenameColumn(
                name: "AthleteCount",
                table: "segment",
                newName: "athlete_count");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "total_elevation_gain",
                table: "segment",
                newName: "TotalElevationGain");

            migrationBuilder.RenameColumn(
                name: "star_count",
                table: "segment",
                newName: "StarCount");

            migrationBuilder.RenameColumn(
                name: "effort_count",
                table: "segment",
                newName: "EffortCount");

            migrationBuilder.RenameColumn(
                name: "athlete_count",
                table: "segment",
                newName: "AthleteCount");
        }
    }
}
