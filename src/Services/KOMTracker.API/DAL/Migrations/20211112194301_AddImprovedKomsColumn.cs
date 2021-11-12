using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KOMTracker.API.DAL.Migrations
{
    public partial class AddImprovedKomsColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "improved_koms",
                table: "koms_summary",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "improved_koms",
                table: "koms_summary");
        }
    }
}
