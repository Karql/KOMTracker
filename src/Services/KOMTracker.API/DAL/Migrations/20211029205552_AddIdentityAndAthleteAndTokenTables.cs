using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace KOMTracker.API.DAL.Migrations;

public partial class AddIdentityAndAthleteAndTokenTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "athlete",
            columns: table => new
            {
                athlete_id = table.Column<int>(type: "integer", nullable: false),
                username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                weight = table.Column<float>(type: "real", nullable: false),
                profile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                profile_medium = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_athlete", x => x.athlete_id);
            });

        migrationBuilder.CreateTable(
            name: "role",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                concurrency_stamp = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_role", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "token",
            columns: table => new
            {
                athlete_id = table.Column<int>(type: "integer", nullable: false),
                token_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                accesstoken = table.Column<string>(name: "access-token", type: "character varying(50)", maxLength: 50, nullable: true),
                refreshtoken = table.Column<string>(name: "refresh-token", type: "character varying(50)", maxLength: 50, nullable: true),
                scope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_token", x => x.athlete_id);
                table.ForeignKey(
                    name: "FK_token_athlete_athlete_id",
                    column: x => x.athlete_id,
                    principalTable: "athlete",
                    principalColumn: "athlete_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                athlete_id = table.Column<int>(type: "integer", nullable: false),
                username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                normalized_username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                password_hash = table.Column<string>(type: "text", nullable: true),
                security_stamp = table.Column<string>(type: "text", nullable: true),
                concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                phone_number = table.Column<string>(type: "text", nullable: true),
                phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                access_failed_count = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user", x => x.id);
                table.ForeignKey(
                    name: "FK_user_athlete_athlete_id",
                    column: x => x.athlete_id,
                    principalTable: "athlete",
                    principalColumn: "athlete_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "role_claim",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                role_id = table.Column<string>(type: "text", nullable: false),
                claim_type = table.Column<string>(type: "text", nullable: true),
                claim_value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_role_claim", x => x.id);
                table.ForeignKey(
                    name: "FK_role_claim_role_role_id",
                    column: x => x.role_id,
                    principalTable: "role",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_claim",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<string>(type: "text", nullable: false),
                claim_type = table.Column<string>(type: "text", nullable: true),
                claim_value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_claim", x => x.id);
                table.ForeignKey(
                    name: "FK_user_claim_user_user_id",
                    column: x => x.user_id,
                    principalTable: "user",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_login",
            columns: table => new
            {
                login_provider = table.Column<string>(type: "text", nullable: false),
                provider_key = table.Column<string>(type: "text", nullable: false),
                provider_display_name = table.Column<string>(type: "text", nullable: true),
                user_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_login", x => new { x.login_provider, x.provider_key });
                table.ForeignKey(
                    name: "FK_user_login_user_user_id",
                    column: x => x.user_id,
                    principalTable: "user",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_role",
            columns: table => new
            {
                user_id = table.Column<string>(type: "text", nullable: false),
                role_id = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_role", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "FK_user_role_role_role_id",
                    column: x => x.role_id,
                    principalTable: "role",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_user_role_user_user_id",
                    column: x => x.user_id,
                    principalTable: "user",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_token",
            columns: table => new
            {
                user_id = table.Column<string>(type: "text", nullable: false),
                login_provider = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                value = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_token", x => new { x.user_id, x.login_provider, x.name });
                table.ForeignKey(
                    name: "FK_user_token_user_user_id",
                    column: x => x.user_id,
                    principalTable: "user",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "role",
            column: "normalized_name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_role_claim_role_id",
            table: "role_claim",
            column: "role_id");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "user",
            column: "normalized_email");

        migrationBuilder.CreateIndex(
            name: "IX_user_athlete_id",
            table: "user",
            column: "athlete_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "user",
            column: "normalized_username",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_user_claim_user_id",
            table: "user_claim",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_user_login_user_id",
            table: "user_login",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_user_role_role_id",
            table: "user_role",
            column: "role_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "role_claim");

        migrationBuilder.DropTable(
            name: "token");

        migrationBuilder.DropTable(
            name: "user_claim");

        migrationBuilder.DropTable(
            name: "user_login");

        migrationBuilder.DropTable(
            name: "user_role");

        migrationBuilder.DropTable(
            name: "user_token");

        migrationBuilder.DropTable(
            name: "role");

        migrationBuilder.DropTable(
            name: "user");

        migrationBuilder.DropTable(
            name: "athlete");
    }
}
