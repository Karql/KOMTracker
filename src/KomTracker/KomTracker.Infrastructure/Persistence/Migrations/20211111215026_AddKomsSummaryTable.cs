using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddKomsSummaryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "koms_summary",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    track_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    athlete_id = table.Column<int>(type: "integer", nullable: false),
                    koms = table.Column<int>(type: "integer", nullable: false),
                    new_koms = table.Column<int>(type: "integer", nullable: false),
                    lost_koms = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_koms_summary", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_koms_summary_athlete_id_track_date",
                table: "koms_summary",
                columns: new[] { "athlete_id", "track_date" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "koms_summary");
        }
    }
}
