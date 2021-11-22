using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddColumnsToKomsSummarySegmentEfforTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "improved_kom",
                table: "koms_summary_segment_effort",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "kom",
                table: "koms_summary_segment_effort",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "lost_kom",
                table: "koms_summary_segment_effort",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "new_kom",
                table: "koms_summary_segment_effort",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "improved_kom",
                table: "koms_summary_segment_effort");

            migrationBuilder.DropColumn(
                name: "kom",
                table: "koms_summary_segment_effort");

            migrationBuilder.DropColumn(
                name: "lost_kom",
                table: "koms_summary_segment_effort");

            migrationBuilder.DropColumn(
                name: "new_kom",
                table: "koms_summary_segment_effort");
        }
    }
}
