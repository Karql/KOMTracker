using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddKomTakeoverTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kom_takeover",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    taken_segment_effort_id = table.Column<long>(type: "bigint", nullable: false),
                    lost_segment_effort_id = table.Column<long>(type: "bigint", nullable: false),
                    taken_koms_summary_id = table.Column<int>(type: "integer", nullable: false),
                    lost_koms_summary_id = table.Column<int>(type: "integer", nullable: false),
                    track_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reverted = table.Column<bool>(type: "boolean", nullable: false),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kom_takeover", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kom_takeover_lost_segment_effort_id",
                table: "kom_takeover",
                column: "lost_segment_effort_id");

            migrationBuilder.CreateIndex(
                name: "IX_kom_takeover_taken_segment_effort_id",
                table: "kom_takeover",
                column: "taken_segment_effort_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kom_takeover");
        }
    }
}
