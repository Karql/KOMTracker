using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddKomsSummarySegmentEffortTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "koms_summary_segment_effort",
                columns: table => new
                {
                    koms_summary_id = table.Column<int>(type: "integer", nullable: false),
                    segment_effort_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_koms_summary_segment_effort", x => new { x.koms_summary_id, x.segment_effort_id });
                    table.ForeignKey(
                        name: "FK_koms_summary_segment_effort_koms_summary_koms_summary_id",
                        column: x => x.koms_summary_id,
                        principalTable: "koms_summary",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_koms_summary_segment_effort_segment_effort_segment_effort_id",
                        column: x => x.segment_effort_id,
                        principalTable: "segment_effort",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_koms_summary_segment_effort_segment_effort_id",
                table: "koms_summary_segment_effort",
                column: "segment_effort_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "koms_summary_segment_effort");
        }
    }
}
