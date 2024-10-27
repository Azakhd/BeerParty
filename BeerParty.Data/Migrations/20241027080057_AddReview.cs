using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавление поля MeetingReviewId в таблицу Likes
            migrationBuilder.AddColumn<long>(
                name: "MeetingReviewId",
                table: "Likes",
                nullable: false,
                defaultValue: 0L);

            // Создание индекса для MeetingReviewId в таблице Likes
            migrationBuilder.CreateIndex(
                name: "IX_Likes_MeetingReviewId",
                table: "Likes",
                column: "MeetingReviewId");

            // Создание внешнего ключа для MeetingReviewId
            migrationBuilder.AddForeignKey(
                name: "FK_Likes_MeetingReviews_MeetingReviewId",
                table: "Likes",
                column: "MeetingReviewId",
                principalTable: "MeetingReviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_MeetingReviews_MeetingReviewId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_MeetingReviewId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "MeetingReviewId",
                table: "Likes");
        }
    }
}
