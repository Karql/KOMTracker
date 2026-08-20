using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentAndWarehouseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "warehouse",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse", x => x.id);
                    table.ForeignKey(
                        name: "FK_warehouse_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "component",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    purchase_place = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    initial_distance_km = table.Column<decimal>(type: "numeric", nullable: false),
                    initial_moving_hours = table.Column<decimal>(type: "numeric", nullable: true),
                    initial_elevation_m = table.Column<decimal>(type: "numeric", nullable: true),
                    warehouse_id = table.Column<int>(type: "integer", nullable: true),
                    lifecycle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sale_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sale_price = table.Column<decimal>(type: "numeric", nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component", x => x.id);
                    table.ForeignKey(
                        name: "FK_component_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_component_warehouse_warehouse_id",
                        column: x => x.warehouse_id,
                        principalSchema: "bt",
                        principalTable: "warehouse",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_component_user_id",
                schema: "bt",
                table: "component",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_component_warehouse_id",
                schema: "bt",
                table: "component",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_user_id",
                schema: "bt",
                table: "warehouse",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "component",
                schema: "bt");

            migrationBuilder.DropTable(
                name: "warehouse",
                schema: "bt");
        }
    }
}
