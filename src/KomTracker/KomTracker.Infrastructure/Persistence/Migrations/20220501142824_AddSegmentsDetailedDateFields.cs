using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddSegmentsDetailedDateFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "segment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "segment",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "segment");
        }
    }
}
