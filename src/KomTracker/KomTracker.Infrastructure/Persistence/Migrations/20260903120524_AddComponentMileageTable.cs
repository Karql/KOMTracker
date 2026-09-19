using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentMileageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "component_mileage",
                schema: "bt",
                columns: table => new
                {
                    component_id = table.Column<int>(type: "integer", nullable: false),
                    total_distance_km = table.Column<decimal>(type: "numeric", nullable: false),
                    total_moving_hours = table.Column<decimal>(type: "numeric", nullable: false),
                    total_elevation_m = table.Column<decimal>(type: "numeric", nullable: false),
                    attributed_activity_count = table.Column<int>(type: "integer", nullable: false),
                    computed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_mileage", x => x.component_id);
                    table.ForeignKey(
                        name: "FK_component_mileage_component_component_id",
                        column: x => x.component_id,
                        principalSchema: "bt",
                        principalTable: "component",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "component_mileage",
                schema: "bt");
        }
    }
}
