using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBikeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bt");

            migrationBuilder.CreateTable(
                name: "bike",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    weight_kg = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    purchase_place = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    initial_distance_km = table.Column<decimal>(type: "numeric", nullable: false),
                    initial_moving_hours = table.Column<decimal>(type: "numeric", nullable: true),
                    initial_elevation_m = table.Column<decimal>(type: "numeric", nullable: true),
                    lifecycle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sale_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sale_price = table.Column<decimal>(type: "numeric", nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bike", x => x.id);
                    table.ForeignKey(
                        name: "FK_bike_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bike_user_id",
                schema: "bt",
                table: "bike",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bike",
                schema: "bt");
        }
    }
}
