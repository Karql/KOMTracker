using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallationParentComponent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "parent_component_id",
                schema: "bt",
                table: "installation",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_installation_parent_component_id",
                schema: "bt",
                table: "installation",
                column: "parent_component_id");

            migrationBuilder.AddForeignKey(
                name: "FK_installation_component_parent_component_id",
                schema: "bt",
                table: "installation",
                column: "parent_component_id",
                principalSchema: "bt",
                principalTable: "component",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_installation_component_parent_component_id",
                schema: "bt",
                table: "installation");

            migrationBuilder.DropIndex(
                name: "IX_installation_parent_component_id",
                schema: "bt",
                table: "installation");

            migrationBuilder.DropColumn(
                name: "parent_component_id",
                schema: "bt",
                table: "installation");
        }
    }
}
