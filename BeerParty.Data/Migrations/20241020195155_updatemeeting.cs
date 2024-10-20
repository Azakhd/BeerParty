using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class updatemeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Meetings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Meetings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Meetings");
        }
    }
}
