using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    place = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cost = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_target",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    bike_id = table.Column<int>(type: "integer", nullable: true),
                    component_id = table.Column<int>(type: "integer", nullable: true),
                    distance_km = table.Column<decimal>(type: "numeric", nullable: true),
                    moving_hours = table.Column<decimal>(type: "numeric", nullable: true),
                    elevation_m = table.Column<decimal>(type: "numeric", nullable: true),
                    computed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_target", x => x.id);
                    table.CheckConstraint("CK_service_target_bike_xor_component", "(bike_id IS NULL) <> (component_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_service_target_bike_bike_id",
                        column: x => x.bike_id,
                        principalSchema: "bt",
                        principalTable: "bike",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_target_component_component_id",
                        column: x => x.component_id,
                        principalSchema: "bt",
                        principalTable: "component",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_target_service_service_id",
                        column: x => x.service_id,
                        principalSchema: "bt",
                        principalTable: "service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_service_user_id",
                schema: "bt",
                table: "service",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_target_bike_id",
                schema: "bt",
                table: "service_target",
                column: "bike_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_target_component_id",
                schema: "bt",
                table: "service_target",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_target_service_id_bike_id",
                schema: "bt",
                table: "service_target",
                columns: new[] { "service_id", "bike_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_target_service_id_component_id",
                schema: "bt",
                table: "service_target",
                columns: new[] { "service_id", "component_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_target",
                schema: "bt");

            migrationBuilder.DropTable(
                name: "service",
                schema: "bt");
        }
    }
}
