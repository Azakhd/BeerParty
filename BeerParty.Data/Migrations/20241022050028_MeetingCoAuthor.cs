using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class MeetingCoAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profiles_Users_UserId1",
                table: "Profiles");

            migrationBuilder.DropIndex(
                name: "IX_Profiles_UserId1",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Profiles");

            migrationBuilder.AddColumn<long>(
                name: "CoAuthorId",
                table: "Meetings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CoAuthorId",
                table: "Meetings",
                column: "CoAuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users_CoAuthorId",
                table: "Meetings",
                column: "CoAuthorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users_CoAuthorId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CoAuthorId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "CoAuthorId",
                table: "Meetings");

            migrationBuilder.AddColumn<long>(
                name: "UserId1",
                table: "Profiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_UserId1",
                table: "Profiles",
                column: "UserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Profiles_Users_UserId1",
                table: "Profiles",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
