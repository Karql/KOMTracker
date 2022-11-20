using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddAthleteStatsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "athlete_stats",
                columns: table => new
                {
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    stats_json = table.Column<string>(type: "json", nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_athlete_stats", x => x.athlete_id);
                    table.ForeignKey(
                        name: "FK_athlete_stats_athlete_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athlete",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "athlete_stats");
        }
    }
}
