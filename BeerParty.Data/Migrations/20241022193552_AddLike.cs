using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
     name: "Likes",
     columns: table => new
     {
         Id = table.Column<long>(nullable: false)
             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn), // Автоинкремент для PostgreSQL
         UserId = table.Column<long>(nullable: false),
         MeetingId = table.Column<long>(nullable: false)
     },
     constraints: table =>
     {
         table.PrimaryKey("PK_Likes", x => x.Id);
         table.ForeignKey(
             name: "FK_Likes_Users_UserId",
             column: x => x.UserId,
             principalTable: "Users",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade);
         table.ForeignKey(
             name: "FK_Likes_Meetings_MeetingId",
             column: x => x.MeetingId,
             principalTable: "Meetings",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade);
     });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_MeetingId",
                table: "Likes",
                column: "MeetingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Likes");
        }
    }
}
