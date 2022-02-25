using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddAuditColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "audit_cd",
                table: "token",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_md",
                table: "token",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_cd",
                table: "segment_effort",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_md",
                table: "segment_effort",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_cd",
                table: "segment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_md",
                table: "segment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_cd",
                table: "koms_summary_segment_effort",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_md",
                table: "koms_summary_segment_effort",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_cd",
                table: "koms_summary",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_md",
                table: "koms_summary",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_cd",
                table: "athlete",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "audit_md",
                table: "athlete",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "audit_cd",
                table: "token");

            migrationBuilder.DropColumn(
                name: "audit_md",
                table: "token");

            migrationBuilder.DropColumn(
                name: "audit_cd",
                table: "segment_effort");

            migrationBuilder.DropColumn(
                name: "audit_md",
                table: "segment_effort");

            migrationBuilder.DropColumn(
                name: "audit_cd",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "audit_md",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "audit_cd",
                table: "koms_summary_segment_effort");

            migrationBuilder.DropColumn(
                name: "audit_md",
                table: "koms_summary_segment_effort");

            migrationBuilder.DropColumn(
                name: "audit_cd",
                table: "koms_summary");

            migrationBuilder.DropColumn(
                name: "audit_md",
                table: "koms_summary");

            migrationBuilder.DropColumn(
                name: "audit_cd",
                table: "athlete");

            migrationBuilder.DropColumn(
                name: "audit_md",
                table: "athlete");
        }
    }
}
