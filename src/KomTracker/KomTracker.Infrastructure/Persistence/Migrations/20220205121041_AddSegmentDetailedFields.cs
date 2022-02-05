using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddSegmentDetailedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AthleteCount",
                table: "segment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EffortCount",
                table: "segment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StarCount",
                table: "segment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalElevationGain",
                table: "segment",
                type: "real",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AthleteCount",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "EffortCount",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "StarCount",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "TotalElevationGain",
                table: "segment");
        }
    }
}
