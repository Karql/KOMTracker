using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalePlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sale_place",
                schema: "bt",
                table: "component",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sale_place",
                schema: "bt",
                table: "bike",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sale_place",
                schema: "bt",
                table: "component");

            migrationBuilder.DropColumn(
                name: "sale_place",
                schema: "bt",
                table: "bike");
        }
    }
}
