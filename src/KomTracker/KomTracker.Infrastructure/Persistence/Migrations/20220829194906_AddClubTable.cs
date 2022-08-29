using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    public partial class AddClubTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "club",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    profile_medium = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    profile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    cover_photo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    cover_photo_small = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    activity_types_icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sport_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    state = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    country = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    @private = table.Column<bool>(name: "private", type: "boolean", nullable: false),
                    member_count = table.Column<int>(type: "integer", nullable: false),
                    featured = table.Column<bool>(type: "boolean", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    audit_cd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audit_md = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "club");
        }
    }
}
