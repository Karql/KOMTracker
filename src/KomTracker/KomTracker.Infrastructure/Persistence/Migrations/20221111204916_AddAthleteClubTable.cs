using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddAthleteClubTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "athlete_club",
                columns: table => new
                {
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    club_id = table.Column<long>(type: "bigint", nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_athlete_club", x => new { x.athlete_id, x.club_id });
                    table.ForeignKey(
                        name: "FK_athlete_club_athlete_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athlete",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_athlete_club_club_club_id",
                        column: x => x.club_id,
                        principalTable: "club",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_athlete_club_club_id",
                table: "athlete_club",
                column: "club_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "athlete_club");
        }
    }
}
