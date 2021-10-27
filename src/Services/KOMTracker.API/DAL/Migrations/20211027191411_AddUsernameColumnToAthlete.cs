using Microsoft.EntityFrameworkCore.Migrations;

namespace KOMTracker.API.DAL.Migrations
{
    public partial class AddUsernameColumnToAthlete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "identity_user",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "normalized_user_name",
                table: "identity_user",
                newName: "normalized_username");

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "strava_athlete",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "username",
                table: "strava_athlete");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "identity_user",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "normalized_username",
                table: "identity_user",
                newName: "normalized_user_name");
        }
    }
}
