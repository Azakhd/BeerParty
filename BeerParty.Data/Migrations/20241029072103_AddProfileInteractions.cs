using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileInteraction_Profiles_FromProfileId",
                table: "ProfileInteraction");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileInteraction_Profiles_ToProfileId",
                table: "ProfileInteraction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfileInteraction",
                table: "ProfileInteraction");

            migrationBuilder.RenameTable(
                name: "ProfileInteraction",
                newName: "ProfileInteractions");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileInteraction_ToProfileId",
                table: "ProfileInteractions",
                newName: "IX_ProfileInteractions_ToProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileInteraction_FromProfileId",
                table: "ProfileInteractions",
                newName: "IX_ProfileInteractions_FromProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfileInteractions",
                table: "ProfileInteractions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileInteractions_Profiles_FromProfileId",
                table: "ProfileInteractions",
                column: "FromProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileInteractions_Profiles_ToProfileId",
                table: "ProfileInteractions",
                column: "ToProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileInteractions_Profiles_FromProfileId",
                table: "ProfileInteractions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileInteractions_Profiles_ToProfileId",
                table: "ProfileInteractions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfileInteractions",
                table: "ProfileInteractions");

            migrationBuilder.RenameTable(
                name: "ProfileInteractions",
                newName: "ProfileInteraction");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileInteractions_ToProfileId",
                table: "ProfileInteraction",
                newName: "IX_ProfileInteraction_ToProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileInteractions_FromProfileId",
                table: "ProfileInteraction",
                newName: "IX_ProfileInteraction_FromProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfileInteraction",
                table: "ProfileInteraction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileInteraction_Profiles_FromProfileId",
                table: "ProfileInteraction",
                column: "FromProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileInteraction_Profiles_ToProfileId",
                table: "ProfileInteraction",
                column: "ToProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
