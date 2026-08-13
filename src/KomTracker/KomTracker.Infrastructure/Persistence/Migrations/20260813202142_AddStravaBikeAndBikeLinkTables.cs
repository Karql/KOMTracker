using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStravaBikeAndBikeLinkTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "bikes_enabled",
                schema: "strava",
                table: "athlete_sync",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "bike",
                schema: "strava",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    nickname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    primary = table.Column<bool>(type: "boolean", nullable: false),
                    retired = table.Column<bool>(type: "boolean", nullable: false),
                    distance = table.Column<double>(type: "double precision", nullable: false),
                    converted_distance = table.Column<double>(type: "double precision", nullable: false),
                    brand_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    model_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    frame_type = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    weight = table.Column<double>(type: "double precision", nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bike1", x => x.id);
                    table.ForeignKey(
                        name: "FK_bike_athlete_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athlete",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bike_link",
                schema: "bt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bike_id = table.Column<int>(type: "integer", nullable: false),
                    external_service = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bike_link", x => x.id);
                    table.ForeignKey(
                        name: "FK_bike_link_bike_bike_id",
                        column: x => x.bike_id,
                        principalSchema: "bt",
                        principalTable: "bike",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bike_athlete_id",
                schema: "strava",
                table: "bike",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "IX_bike_link_bike_id",
                schema: "bt",
                table: "bike_link",
                column: "bike_id");

            migrationBuilder.CreateIndex(
                name: "IX_bike_link_external_service_external_id",
                schema: "bt",
                table: "bike_link",
                columns: new[] { "external_service", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bike",
                schema: "strava");

            migrationBuilder.DropTable(
                name: "bike_link",
                schema: "bt");

            migrationBuilder.DropColumn(
                name: "bikes_enabled",
                schema: "strava",
                table: "athlete_sync");
        }
    }
}
