using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingParticipants_Meetings_MeetingId1",
                table: "MeetingParticipants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants");

            migrationBuilder.DropIndex(
                name: "IX_MeetingParticipants_MeetingId",
                table: "MeetingParticipants");

            migrationBuilder.DropIndex(
                name: "IX_MeetingParticipants_MeetingId1",
                table: "MeetingParticipants");

            migrationBuilder.DropColumn(
                name: "MeetingId1",
                table: "MeetingParticipants");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "MeetingParticipants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants",
                columns: new[] { "MeetingId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "MeetingParticipants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<long>(
                name: "MeetingId1",
                table: "MeetingParticipants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeetingParticipants",
                table: "MeetingParticipants",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingParticipants_MeetingId",
                table: "MeetingParticipants",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingParticipants_MeetingId1",
                table: "MeetingParticipants",
                column: "MeetingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingParticipants_Meetings_MeetingId1",
                table: "MeetingParticipants",
                column: "MeetingId1",
                principalTable: "Meetings",
                principalColumn: "Id");
        }
    }
}
