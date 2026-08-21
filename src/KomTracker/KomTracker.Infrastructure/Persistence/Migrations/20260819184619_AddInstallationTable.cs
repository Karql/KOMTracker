using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "installation",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    component_id = table.Column<int>(type: "integer", nullable: false),
                    bike_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    date_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    manual_distance_km = table.Column<decimal>(type: "numeric", nullable: true),
                    manual_moving_hours = table.Column<decimal>(type: "numeric", nullable: true),
                    manual_elevation_m = table.Column<decimal>(type: "numeric", nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_installation", x => x.id);
                    table.ForeignKey(
                        name: "FK_installation_bike_bike_id",
                        column: x => x.bike_id,
                        principalSchema: "bt",
                        principalTable: "bike",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_installation_component_component_id",
                        column: x => x.component_id,
                        principalSchema: "bt",
                        principalTable: "component",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_installation_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_installation_bike_id",
                schema: "bt",
                table: "installation",
                column: "bike_id");

            migrationBuilder.CreateIndex(
                name: "IX_installation_component_id",
                schema: "bt",
                table: "installation",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_installation_user_id",
                schema: "bt",
                table: "installation",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "installation",
                schema: "bt");
        }
    }
}
