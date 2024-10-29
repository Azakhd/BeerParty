using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeBioAndLocationNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "Profiles",
                nullable: true, // Изменяем на nullable
                oldClrType: typeof(string),
                oldType: "text"); // Предположим, что старый тип text

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Profiles",
                nullable: true, // Изменяем на nullable
                oldClrType: typeof(string),
                oldType: "text"); // Предположим, что старый тип text
        }
    }
}
