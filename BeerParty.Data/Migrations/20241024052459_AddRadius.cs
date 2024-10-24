using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRadius : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Radius",
                table: "Meetings",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Radius",
                table: "Meetings");
        }
    }
}
